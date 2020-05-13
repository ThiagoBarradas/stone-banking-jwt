using StoneBanking.Jwt.Models;
using System.Collections.Generic;

namespace StoneBanking.Jwt
{
    public interface IStoneBankingJwt
    {
        /// <summary>
        /// Creates new consent url based on your rsa key 
        /// </summary>
        /// <returns>redirect url</returns>
        string CreateConsentUrl();

        /// <summary>
        /// Creates new consent url based on your rsa key 
        /// </summary>
        /// <param name="metadata">additional data</param>
        /// <returns>redirect url</returns>
        string CreateConsentUrl(Dictionary<string, string> metadata);

        /// <summary>
        /// Creates new authentication token from your rsa key 
        /// You need to use authentication token to generate a access token
        /// </summary>
        /// <returns></returns>
        string CreateAuthenticationToken();

        /// <summary>
        /// Creates new access token or reuse when previous is not expired
        /// </summary>
        /// <returns>access token obj</returns>
        AccessTokenResponse CreateAccessToken();

        /// <summary>
        /// Creates new access token based on authentication token
        /// </summary>
        /// <param name="authenticationToken">authentication token obtained from CreateAuthenticationToken()</param>
        /// <returns>access token obj</returns>
        AccessTokenResponse CreateAccessToken(string authenticationToken);

        /// <summary>
        /// Decodes a jwt token encrypted with your rsa key pair
        /// </summary>
        /// <returns>decoded obj as dictionary</returns>
        Dictionary<string, object> DecodeToken(string token);
    }
}
