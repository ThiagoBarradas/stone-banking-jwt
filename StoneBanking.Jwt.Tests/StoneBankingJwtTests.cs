using Microsoft.Extensions.DependencyInjection;
using StoneBanking.Jwt.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Xunit;

namespace StoneBanking.Jwt.Tests
{
    public static class StoneBankingJwtTests
    {
        [Fact]
        public static void CreateProductionConsentUrl_WithoutParams()
        {
            // arrange
            var now_timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByString", null);
            var services = new ServiceCollection();
            services.AddStoneBankingJwt(configuration);
            var jwtSign = services.BuildServiceProvider().GetService<IStoneBankingJwt>();

            // act
            var url = jwtSign.CreateConsentUrl();
            var query = ParseQueryString(url.Split("?").LastOrDefault());
            var decodedToken = jwtSign.DecodeToken(query["jwt"]);

            // assert
            Assert.Equal(query.Count, 2);
            Assert.Equal(query["client_id"], "321321321");
            Assert.NotNull(query["jwt"]);
            Assert.Equal(url.Split("?").FirstOrDefault(), "https://accounts.openbank.stone.com.br/consentimento");

            var aud = decodedToken["aud"].ToString();
            var clientId = decodedToken["client_id"].ToString();
            var iss = decodedToken["iss"].ToString();
            var redirect_uri = decodedToken["redirect_uri"].ToString();

            Assert.Equal(aud, "accounts-hubid@openbank.stone.com.br");
            Assert.Equal(clientId, "321321321");
            Assert.Equal(iss, "321321321");
            Assert.Equal(redirect_uri, "https://mysite.com/stonebanking/success2");
        }

        [Fact]
        public static void CreateConsentUrl_WithoutParams()
        {
            // arrange
            var now_timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile", null);
            var services = new ServiceCollection();
            services.AddStoneBankingJwt(configuration);
            var jwtSign = services.BuildServiceProvider().GetService<IStoneBankingJwt>();

            // act
            var url = jwtSign.CreateConsentUrl();
            var query = ParseQueryString(url.Split("?").LastOrDefault());
            var decodedToken = jwtSign.DecodeToken(query["jwt"]);

            // assert
            Assert.Equal(query.Count, 2);
            Assert.Equal(query["client_id"], "123123123");
            Assert.NotNull(query["jwt"]);
            Assert.Equal(url.Split("?").FirstOrDefault(), "https://sandbox-accounts.openbank.stone.com.br/consentimento");

            var aud = decodedToken["aud"].ToString();
            var clientId = decodedToken["client_id"].ToString();
            var iss = decodedToken["iss"].ToString();
            var exp = int.Parse(decodedToken["exp"].ToString());
            var iat = int.Parse(decodedToken["iat"].ToString());
            var nbf = int.Parse(decodedToken["nbf"].ToString());
            var jti = int.Parse(decodedToken["jti"].ToString());
            var redirect_uri = decodedToken["redirect_uri"].ToString();

            Assert.Equal(aud, "accounts-hubid@openbank.stone.com.br");
            Assert.Equal(clientId, "123123123");
            Assert.Equal(iss, "123123123");
            Assert.True(exp >= now_timestamp + 300 && exp <= now_timestamp + 100 + 300);
            Assert.True(iat >= now_timestamp && iat <= now_timestamp + 100);
            Assert.True(nbf >= now_timestamp && nbf <= now_timestamp + 100);
            Assert.True(jti >= now_timestamp && nbf <= now_timestamp + 100);
            Assert.Equal(redirect_uri, "https://mysite.com/stonebanking/success");
        }

        [Fact]
        public static void CreateConsentUrl_WithMetadata()
        {
            // arrange
            var now_timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile", null);
            var services = new ServiceCollection();
            services.AddStoneBankingJwt(configuration);
            var jwtSign = services.BuildServiceProvider().GetService<IStoneBankingJwt>();

            // act
            var metadata = new Dictionary<string, string>
            {
                { "user_id", "user_123" },
                { "other_data", "123123" } 
            };
            var url = jwtSign.CreateConsentUrl(metadata);
            var query = ParseQueryString(url.Split("?").LastOrDefault());
            var decodedToken = jwtSign.DecodeToken(query["jwt"]);

            // assert
            Assert.Equal(query.Count, 2);
            Assert.Equal(query["client_id"], "123123123");
            Assert.NotNull(query["jwt"]);
            Assert.Equal(url.Split("?").FirstOrDefault(), "https://sandbox-accounts.openbank.stone.com.br/consentimento");

            var aud = decodedToken["aud"].ToString();
            var clientId = decodedToken["client_id"].ToString();
            var iss = decodedToken["iss"].ToString();
            var exp = int.Parse(decodedToken["exp"].ToString());
            var iat = int.Parse(decodedToken["iat"].ToString());
            var nbf = int.Parse(decodedToken["nbf"].ToString());
            var jti = int.Parse(decodedToken["jti"].ToString());
            var redirect_uri = decodedToken["redirect_uri"].ToString();
            var sessionMetadata = (Dictionary<string, object>) decodedToken["session_metadata"];

            Assert.Equal(aud, "accounts-hubid@openbank.stone.com.br");
            Assert.Equal(clientId, "123123123");
            Assert.Equal(iss, "123123123");
            Assert.True(exp >= now_timestamp + 300 && exp <= now_timestamp + 100 + 300);
            Assert.True(iat >= now_timestamp && iat <= now_timestamp + 100);
            Assert.True(nbf >= now_timestamp && nbf <= now_timestamp + 100);
            Assert.True(jti >= now_timestamp && nbf <= now_timestamp + 100);
            Assert.Equal(redirect_uri, "https://mysite.com/stonebanking/success");
            Assert.Equal(sessionMetadata["user_id"], "user_123");
            Assert.Equal(sessionMetadata["other_data"], "123123");
            Assert.Equal(sessionMetadata.Count, 2);
        }

        [Fact]
        public static void CreateAuthenticationToken()
        {
            // arrange
            var now_timestamp = ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds();
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile", null);
            var services = new ServiceCollection();
            services.AddStoneBankingJwt(configuration);
            var jwtSign = services.BuildServiceProvider().GetService<IStoneBankingJwt>();

            // act
            var token = jwtSign.CreateAuthenticationToken();
            var decodedToken = jwtSign.DecodeToken(token);

            // assert
            var aud = decodedToken["aud"].ToString();
            var clientId = decodedToken["clientId"].ToString();
            var exp = int.Parse(decodedToken["exp"].ToString());
            var iat = int.Parse(decodedToken["iat"].ToString());
            var nbf = int.Parse(decodedToken["nbf"].ToString());
            var jti = int.Parse(decodedToken["jti"].ToString());
            var realm = decodedToken["realm"].ToString();
            var sub = decodedToken["sub"].ToString();

            Assert.Equal(aud, "https://sandbox-accounts.openbank.stone.com.br/auth/realms/stone_bank");
            Assert.Equal(clientId, "123123123");
            Assert.True(exp >= now_timestamp + 900 && exp <= now_timestamp + 100 + 900);
            Assert.True(iat >= now_timestamp       && iat <= now_timestamp + 100);
            Assert.True(nbf >= now_timestamp       && nbf <= now_timestamp + 100);
            Assert.True(jti >= now_timestamp       && nbf <= now_timestamp + 100);
            Assert.Equal(realm, "stone_bank");
            Assert.Equal(sub, "123123123");
        }

        [Fact]
        public static void DecodeAuthenticationToken()
        {
            // arrange
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJodHRwczovL3NhbmRib3gtYWNjb3VudHMub3BlbmJhbmsuc3RvbmUuY29tLmJyL2F1dGgvcmVhbG1zL3N0b25lX2JhbmsiLCJjbGllbnRJZCI6IjEyMzEyMzEyMyIsImV4cCI6MTU4ODk0OTE1MiwiaWF0IjoxNTg4OTQ4MjUyLCJuYmYiOjE1ODg5NDgyNTIsImp0aSI6IjE1ODg5NDgyNTIiLCJyZWFsbSI6InN0b25lX2JhbmsiLCJzdWIiOiIxMjMxMjMxMjMifQ.0rRZyf3f6nETHOfn-DBR2fa38qJ09fZsxt-bZcT7XuNEENXx8ji-IddNBCDqltCuR9t8Jwc_e0qSOH3r3OHGX3XsafftOvhiByZV_iL74IpljVKPDBAEMlEEMsO4irBO6POKEUW65bD3TMSt-sdlC11r28Vi-zdI0F3V8ERGlXzfdwxbVL1WzTaDi54WeGHuhoHOI_gNEJ4LJkVCracn1RtM8AgSmPT8yIHt-IovyXRDeogSgifzxSwskRThRGj4GncTG93xbAL1-_G9XZjp6VTCCr1Ujky_QTrkWyg59JVUPLnFCxNRwW1OXJyQqf2yF8F5mE7CSBy2prVCh-J50YHdjRbXeIm0h3MGKnkM0kRpuv4U-dmauyooi6qGE52oWgZeR0mqpiDK81_01zEim6xLu_WXQs1Sf1Dq13YUjymI24SjbugAFpMgaBsclpAfwpbcLeTqYlO8Xnyv_q6lCziBeRUUPRcMMf6d_epxZe-h781lGundLmDmYn_purXfOodTe9fey1GZw2Yfn1ce0E4kPP-joTZkhoolx34I-G2kvGnRMNbtX9r99U4XGUOSnfv1gnVRcfrOK3g-VBJJB_5NARWa0SdNu7VF4rPL-iubjq_WRUUUzxTOF_Ed6-2Py9nRiZUpzW--CPXkHochor_ArIt6xlfRLN10Ys0khEA";
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile", null);
            var services = new ServiceCollection();
            services.AddStoneBankingJwt(configuration);
            var jwtSign = services.BuildServiceProvider().GetService<IStoneBankingJwt>();

            // act
            var decodedToken = jwtSign.DecodeToken(token);

            // assert
            var aud = decodedToken["aud"].ToString();
            var clientId = decodedToken["clientId"].ToString();
            var exp = int.Parse(decodedToken["exp"].ToString());
            var iat = int.Parse(decodedToken["iat"].ToString());
            var nbf = int.Parse(decodedToken["nbf"].ToString());
            var jti = decodedToken["jti"].ToString();
            var realm = decodedToken["realm"].ToString();
            var sub = decodedToken["sub"].ToString();

            Assert.Equal(aud, "https://sandbox-accounts.openbank.stone.com.br/auth/realms/stone_bank");
            Assert.Equal(clientId, "123123123");
            Assert.Equal(exp, 1588949152);
            Assert.Equal(iat, 1588948252);
            Assert.Equal(nbf, 1588948252);
            Assert.Equal(jti, "1588948252");
            Assert.Equal(realm, "stone_bank");
            Assert.Equal(sub, "123123123");
        }

        [Fact]
        public static void DecodeConsentToken()
        {
            // arrange
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0eXBlIjoiY29uc2VudCIsImF1ZCI6ImFjY291bnRzLWh1YmlkQG9wZW5iYW5rLnN0b25lLmNvbS5iciIsImNsaWVudF9pZCI6IjEyMzEyMzEyMyIsImlzcyI6IjEyMzEyMzEyMyIsImV4cCI6MTU4ODk1NTIzMywiaWF0IjoxNTg4OTU0OTMzLCJuYmYiOjE1ODg5NTQ5MzMsImp0aSI6IjE1ODg5NTQ5MzMiLCJyZWRpcmVjdF91cmkiOiJodHRwczovL215c2l0ZS5jb20vc3RvbmViYW5raW5nL3N1Y2Nlc3MiLCJzZXNzaW9uX21ldGFkYXRhIjp7InVzZXJfaWQiOiJ1c2VyXzEyMyIsIm90aGVyX2RhdGEiOiIxMjMxMjMifX0.2lAGMIGD3yOuIoAY7k-Fs3X2JAYb0VgyCkxgUPaJk6m7xJH0AH0NZODpVNnK-tUoI_CS17y_fAgrNRgFQ-lHmK3UptFupvmlRs-TBjdDyIsWos5aZFCYio4bizkTE2rZbOqwk5_ou6-nsdhxKJy8-yEeOuHA9yImzknqcSaLhLBI7r6wJ_O8RLd4s3KpE7vsbiAOoojF3JTZzzKpbRmz2iGro1-hVmCb36m_I9X_qc2H3G8SEEdMttBqT9i7BfChLt7hGD1D7l5TZtc5UxajgM6lCXCinUepKxN9Q0BKgchdyEyygZyKBf4-awnF-ez34YX2dBMINmFQ_6cewEAGFEi6S2tj8coap4Jz9Orv1NMmrXVMvYqNRKfCPsXAFU8FolPAvGjrc3uoxFrBT9D1HvV8qVYzAFWjp-dio8LNsOdeGxPIua6cTY82P3U2A6WMiXvbjsCNRQEsMEQUmN-bLaGRFdpxTUT9b0gc0fOpzgHXzgP1qsnPwNqzNfMl-I0wpKIyzEzxQopbA-EsrPrnl_RPASMsN760jIiCcTz-zNdtFDTd2Jm4GwuErpyvhjVoFfxhzirH3zKghTiI7AWT7pPF-EnWNUfZvMtQgUo2l-aLVtxMg1Bcyuf3Hmwmij9_N1vdl4789pDRri3bFw0nFTHVwr_IHY2aAXtvFeirySI";
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile", null);
            var services = new ServiceCollection();
            services.AddStoneBankingJwt(configuration);
            var jwtSign = services.BuildServiceProvider().GetService<IStoneBankingJwt>();

            // act
            var decodedToken = jwtSign.DecodeToken(token);

            // assert
            var aud = decodedToken["aud"].ToString();
            var clientId = decodedToken["client_id"].ToString();
            var iss = decodedToken["iss"].ToString();
            var exp = int.Parse(decodedToken["exp"].ToString());
            var iat = int.Parse(decodedToken["iat"].ToString());
            var nbf = int.Parse(decodedToken["nbf"].ToString());
            var jti = int.Parse(decodedToken["jti"].ToString());
            var redirect_uri = decodedToken["redirect_uri"].ToString();
            var sessionMetadata = (Dictionary<string, object>)decodedToken["session_metadata"];

            Assert.Equal(aud, "accounts-hubid@openbank.stone.com.br");
            Assert.Equal(clientId, "123123123");
            Assert.Equal(iss, "123123123");
            Assert.Equal(exp, 1588955233);
            Assert.Equal(iat, 1588954933);
            Assert.Equal(nbf, 1588954933);
            Assert.Equal(jti, 1588954933);
            Assert.Equal(redirect_uri, "https://mysite.com/stonebanking/success");
            Assert.Equal(sessionMetadata["user_id"], "user_123");
            Assert.Equal(sessionMetadata["other_data"], "123123");
            Assert.Equal(sessionMetadata.Count, 2);
        }

        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }
    }
}
