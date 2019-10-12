using AutoWrapper.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace AutoWrapper.Helpers
{
    internal class CamelCaseContractResolverJsonSettings : IJsonSettings
    {
        public JsonSerializerSettings GetJSONSettings(bool ignoreNull)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
            };
        }
    }

    internal class CustomContractResolverJsonSettings<T> : IJsonSettings
    {
        public JsonSerializerSettings GetJSONSettings(bool ignoreNull)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver<T>(),//{ NamingStrategy = new CamelCaseNamingStrategy() { OverrideSpecifiedNames = false }  },
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
            };
        }
    }


    internal static class JSONHelper
    {
        public static JsonSerializerSettings GetJSONSettings(bool ignoreNull = true)
        {
            return new CamelCaseContractResolverJsonSettings().GetJSONSettings(ignoreNull);
        }

        public static JsonSerializerSettings GetJSONSettings<T>(bool ignoreNull = true)
        {
            return new CustomContractResolverJsonSettings<T>().GetJSONSettings(ignoreNull);
        }
    }
}
