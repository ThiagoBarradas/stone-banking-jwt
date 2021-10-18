namespace StoneBanking.Jwt.Models
{
    public class StoneBankingSettings
    {
        public string ClientId { get; set; }

        public StoneBankingEnvironment Environment { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public int AuthenticationExpiresInSeconds { get; set; } = 900;

        public int ConsentExpiresInSeconds { get; set; } = 300;

        public string ConsentDefaultRedirectUrl { get; set; }
    }

    public static class StoneBankingSettingsStatic
    {
        public static string AccountsApiSandbox => "https://sandbox.conta.stone.com.br";

        public static string AccountsApiProduction => "https://conta.stone.com.br";

        public static string GetAccountsApi(StoneBankingEnvironment environment)
        {
            return (environment == StoneBankingEnvironment.Production)
                ? AccountsApiProduction
                : AccountsApiSandbox;
        }

        public static string AuthenticationApiSandbox => "https://sandbox-api.openbank.stone.com.br";

        public static string AuthenticationApiProduction => "https://api.openbank.stone.com.br";

        public static string GetAuthenticationApi(StoneBankingEnvironment environment)
        {
            return (environment == StoneBankingEnvironment.Production)
                ? AuthenticationApiSandbox
                : AuthenticationApiProduction;
        }

        public static string AuthenticationAud => "https://sandbox.conta.stone.com.br/auth/realms/stone_bank";

        public static string AuthenticationRealms => "stone_bank";

        public static string ConsentType => "consent";

        public static string ConsentAud => "accounts-hubid@openbank.stone.com.br";
    }
}
