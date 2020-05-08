using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace StoneBanking.Jwt.Extensions
{
    public static class JsonAsDictionaryExtension
    {
        public static Dictionary<string, object> JsonToDictionary(this string json)
        {
            return (Dictionary<string, object>) DeserializeAsObjectCore(JToken.Parse(json));
        }

        private static object DeserializeAsObjectCore(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => DeserializeAsObjectCore(prop.Value));

                case JTokenType.Array:
                    return token.Select(DeserializeAsObjectCore).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }
    }
}
