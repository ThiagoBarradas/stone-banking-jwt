using Newtonsoft.Json;

namespace StoneBanking.Jwt.Models
{
    public class StoneBankingErrorResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}
