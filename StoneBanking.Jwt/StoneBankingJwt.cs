using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using StoneBanking.Jwt.Extensions;
using StoneBanking.Jwt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;

namespace StoneBanking.Jwt
{
    public class StoneBankingJwt : IStoneBankingJwt
    {
        private object Lock = new object();

        public DateTime ExpiresIn { get; private set; } = DateTime.UtcNow;

        public AccessTokenResponse CurrentAccessToken { get; private set; } = new AccessTokenResponse();

        public StoneBankingSettings StoneBankingSettings { get; private set; }

        public string PrivateKey { get; private set; }

        public string PublicKey { get; private set; }

        public string AssemblyVersion { get; private set; }

        public string AccountsApiUrl { get; private set; }

        public StoneBankingJwt(StoneBankingSettings stoneBankingSettings)
        {
            this.StoneBankingSettings = stoneBankingSettings;
            this.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.AccountsApiUrl = StoneBankingSettingsStatic.GetAccountsApi(this.StoneBankingSettings.Environment);

            this.ValidateRequiredSettings();
            this.ReadPublicKey();
            this.ReadPrivateKey();
        }

        public string CreateConsentUrl()
        {
            return this.CreateConsentUrl(null);
        }

        public string CreateConsentUrl(Dictionary<string, string> metadata)
        {
            var token = this.CreateToken(this.GetConsentPayload(metadata));
            var clientId = this.StoneBankingSettings.ClientId;

            return $"{this.AccountsApiUrl}/consentimento?client_id={clientId}&jwt={token}";
        }

        public string CreateAuthenticationToken()
        {
            return this.CreateToken(this.GetAuthenticationPayload());
        }

        public AccessTokenResponse CreateAccessToken()
        {
            lock (Lock)
            {
                var now = DateTime.UtcNow;
                if (now < this.CurrentAccessToken.ExpiresInDate)
                {
                    return this.CurrentAccessToken;
                }

                var authenticationToken = this.CreateAuthenticationToken();
                this.CurrentAccessToken = this.CreateAccessToken(authenticationToken);
            }

            return this.CurrentAccessToken;
        }
        
        public AccessTokenResponse CreateAccessToken(string authenticationToken)
        {
            var url = $"{this.AccountsApiUrl}/auth/realms/stone_bank/protocol/openid-connect/token";

            var body = new List<KeyValuePair<string, string>>();
            body.Add(new KeyValuePair<string, string>("client_id", this.StoneBankingSettings.ClientId));
            body.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            body.Add(new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));
            body.Add(new KeyValuePair<string, string>("client_assertion", authenticationToken));

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(body)
            };

            request.Headers.Add("Accept", "application/json");
            ProductHeaderValue header = new ProductHeaderValue("StoneBanking.Jwt", this.AssemblyVersion);
            ProductInfoHeaderValue userAgent = new ProductInfoHeaderValue(header);
            request.Headers.UserAgent.Add(userAgent);

            var json = "{}";
            using (var client = new HttpClient())
            {
                var response = client.SendAsync(request).Result;
                
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    json = response.Content.ReadAsStringAsync().Result;
                    var error = JsonConvert.DeserializeObject<StoneBankingErrorResponse>(json);
                    throw new HttpRequestException($"BadRequest: {error.ErrorDescription}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Invalid status code received from StoneBanking api: {response.StatusCode}");
                }

                json = response.Content.ReadAsStringAsync().Result;
            }

            var accessToken = JsonConvert.DeserializeObject<AccessTokenResponse>(json);

            return accessToken;
        }

        public Dictionary<string, object> DecodeToken(string token)
        {
            var json = this.DecodeTokenAsJson(token);

            return json.JsonToDictionary();
        }

        private string DecodeTokenAsJson(string token)
        {
            RSAParameters rsaParams;

            using (var tr = new StringReader(this.PublicKey))
            {
                var pemReader = new PemReader(tr);
                var publicKeyParams = pemReader.ReadObject() as RsaKeyParameters;
                if (publicKeyParams == null)
                {
                    throw new ArgumentException("Could not read RSA PublicKey, please check your keys.\n See how generate a valid key: https://gist.github.com/ThiagoBarradas/e58ac282665306977777ffa3f32df376");
                }
                rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParams);
            }
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);
                return Jose.JWT.Decode(token, rsa, Jose.JwsAlgorithm.RS256);
            }
        }

        private string CreateToken(Dictionary<string, object> payload)
        {
            RSAParameters rsaParams;
            using (var tr = new StringReader(this.PrivateKey))
            {
                var pemReader = new PemReader(tr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                if (keyPair == null)
                {
                    throw new ArgumentException("Could not read RSA PrivateKey, please check your keys.\n See how generate a valid key: https://gist.github.com/ThiagoBarradas/e58ac282665306977777ffa3f32df376");
                }
                var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);
                return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256);
            }
        }

        private Dictionary<string, object> GetAuthenticationPayload()
        {
            var now = DateTime.UtcNow;
            var now_timestamp = ((DateTimeOffset) now).ToUnixTimeSeconds();
            var exp_timestamp = ((DateTimeOffset) now.AddSeconds(this.StoneBankingSettings.AuthenticationExpiresInSeconds)).ToUnixTimeSeconds();

            var payload = new Dictionary<string, object>
            {
                { "aud", StoneBankingSettingsStatic.AuthenticationAud },
                { "clientId", this.StoneBankingSettings.ClientId },
                { "exp", exp_timestamp },
                { "iat", now_timestamp },
                { "nbf", now_timestamp },
                { "jti", now_timestamp.ToString() },
                { "realm", StoneBankingSettingsStatic.AuthenticationRealms },
                { "sub", this.StoneBankingSettings.ClientId }
            };

            return payload;
        }

        private Dictionary<string, object> GetConsentPayload(Dictionary<string, string> metadata)
        {
            var now = DateTime.UtcNow;
            var now_timestamp = ((DateTimeOffset) now).ToUnixTimeSeconds();
            var exp_timestamp = ((DateTimeOffset) now.AddSeconds(this.StoneBankingSettings.ConsentExpiresInSeconds)).ToUnixTimeSeconds();

            var payload = new Dictionary<string, object>
            {
                { "type", StoneBankingSettingsStatic.ConsentType },
                { "aud", StoneBankingSettingsStatic.ConsentAud },
                { "client_id", this.StoneBankingSettings.ClientId },
                { "iss", this.StoneBankingSettings.ClientId },
                { "exp", exp_timestamp },
                { "iat", now_timestamp },
                { "nbf", now_timestamp },
                { "jti", now_timestamp.ToString() },
                { "redirect_uri", this.StoneBankingSettings.ConsentDefaultRedirectUrl }
            };

            if (metadata?.Any() == true)
            {
                payload.Add("session_metadata", metadata);
            }

            return payload;
        }

        private void ValidateRequiredSettings()
        {
            if (this.StoneBankingSettings == null)
            {
                throw new ArgumentNullException("StoneBankingSettings cannot be null.");
            }

            if (this.StoneBankingSettings.ClientId == null)
            {
                throw new ArgumentNullException("ClientId cannot be null.\n See how to create a application https://docs.openbank.stone.com.br/docs/cadastro-da-aplicacao-guides");
            }

            if (this.StoneBankingSettings.PublicKey == null)
            {
                throw new ArgumentNullException("PublicKey cannot be null.\n See how generate a valid key: https://gist.github.com/ThiagoBarradas/e58ac282665306977777ffa3f32df376");
            }

            if (this.StoneBankingSettings.PrivateKey == null)
            {
                throw new ArgumentNullException("PrivateKey cannot be null.\n See how generate a valid key: https://gist.github.com/ThiagoBarradas/e58ac282665306977777ffa3f32df376");
            }
        }

        private void ReadPublicKey()
        {
            this.PublicKey =
                this.ReadKey(this.StoneBankingSettings.PublicKey, "PublicKey", "BEGIN PUBLIC KEY");
        }

        private void ReadPrivateKey()
        {
            this.PrivateKey = 
                this.ReadKey(this.StoneBankingSettings.PrivateKey, "PrivateKey", "BEGIN RSA PRIVATE KEY");
        }

        public string ReadKey(string key, string name, string contains)
        {
            string keyContent;

            if (key.Contains(contains))
            {
                keyContent = key;
            }
            else
            {
                if (!File.Exists(key))
                {
                    throw new ArgumentException($"{name} must be a string with key or a path to valid file with key.\n See how to generate a valid key: https://gist.github.com/ThiagoBarradas/e58ac282665306977777ffa3f32df376");
                }

                keyContent = File.ReadAllText(key);
            }

            return HandleBreakLine(keyContent);
        }

        private static string HandleBreakLine(string content)
        {
            return content?.Replace("\\n", "\n")
                           .Replace("\\r", "\r")
                           .Replace("\r", "");
        }
    }
}
