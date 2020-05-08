using System.Collections.Generic;

namespace StoneBanking.Jwt
{
    public interface IStoneBankingJwt
    {
        string CreateConsentUrl();

        string CreateConsentUrl(Dictionary<string, string> metadata);

        string CreateConsentUrl(string redirectUrl);

        string CreateConsentUrl(Dictionary<string, string> metadata, string redirectUrl);


        string CreateAuthenticationToken();

        Dictionary<string, object> DecodeToken(string token);
    }
}
