using System.Text.Json;

namespace AutoWrapper.Configurations
{
    public static class JsonSettingsConfiguration
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
