using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoWrapper.Configurations
{
    public static class JsonSettings
    {
        public static JsonSerializerOptions GetJsonSerializerOptions(JsonNamingPolicy jsonNamingPolicy, bool ignoreNullValues = true)
            =>  new JsonSerializerOptions
                {
                    IgnoreNullValues = ignoreNullValues,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = jsonNamingPolicy,
                    WriteIndented = true
            };
    }
}
