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
using System.Security.Cryptography;

namespace StoneBanking.Jwt
{
    public class StoneBankingJwt : IStoneBankingJwt
    {
        public StoneBankingSettings StoneBankingSettings { get; set; }

        public string PrivateKey { get; set; }

        public string PublicKey { get; set; }

        public StoneBankingJwt(StoneBankingSettings stoneBankingSettings)
        {
            this.StoneBankingSettings = stoneBankingSettings;

            this.ValidateRequiredSettings();
            this.ReadPublicKey();
            this.ReadPrivateKey();
        }

        public string CreateConsentUrl()
        {
            return this.CreateConsentUrl(null, null);
        }

        public string CreateConsentUrl(Dictionary<string, string> metadata)
        {
            return this.CreateConsentUrl(metadata, null);
        }

        public string CreateConsentUrl(string redirectUrl)
        {
            return this.CreateConsentUrl(null, redirectUrl);
        }

        public string CreateConsentUrl(Dictionary<string, string> metadata, string redirectUrl)
        {
            var token = this.CreateToken(this.GetConsentPayload(redirectUrl, metadata));
            var clientId = this.StoneBankingSettings.ClientId;
            var baseUrl = StoneBankingSettingsStatic.GetAccountsApi(this.StoneBankingSettings.Environment);

            return $"{baseUrl}/#/consent?type=consent&client_id={clientId}&jwt={token}";
        }

        public string CreateAuthenticationToken()
        {
            return this.CreateToken(this.GetAuthenticationPayload());
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
                    throw new Exception("Could not read RSA public key");
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
                    throw new Exception("Could not read RSA private key");
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

            var payload = new Dictionary<string, object>()
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

        private Dictionary<string, object> GetConsentPayload(string redirectUrl, Dictionary<string, string> metadata)
        {
            var now = DateTime.UtcNow;
            var now_timestamp = ((DateTimeOffset) now).ToUnixTimeSeconds();
            var exp_timestamp = ((DateTimeOffset) now.AddSeconds(this.StoneBankingSettings.ConsentExpiresInSeconds)).ToUnixTimeSeconds();

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                redirectUrl = this.StoneBankingSettings.ConsentDefaultRedirectUrl;
            }

            var payload = new Dictionary<string, object>()
            {
                { "type", StoneBankingSettingsStatic.ConsentType },
                { "aud", StoneBankingSettingsStatic.ConsentAud },
                { "client_id", this.StoneBankingSettings.ClientId },
                { "iss", this.StoneBankingSettings.ClientId },
                { "exp", exp_timestamp },
                { "iat", now_timestamp },
                { "nbf", now_timestamp },
                { "jti", now_timestamp.ToString() },
                { "redirect_uri", redirectUrl }
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
                throw new ArgumentNullException("ClientId cannot be null. See how to create a application https://docs.openbank.stone.com.br/docs/cadastro-da-aplicacao-guides");
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

            return this.HandleBreakLine(keyContent);
        }

        private string HandleBreakLine(string content)
        {
            return content?.Replace("\\n", "\n")
                           .Replace("\\r", "\r")
                           .Replace("\r", "");
        }
    }
}
