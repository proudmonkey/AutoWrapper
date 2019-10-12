using AutoWrapper.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace AutoWrapper.Helpers
{
    internal class CamelCaseContractResolverJsonSettings : IJsonSettings
    {
        public JsonSerializerSettings GetJSONSettings(bool ignoreNull, bool useCamelCaseNaming = true)
        {
            return new JsonSerializerSettings
            {
           
                ContractResolver = useCamelCaseNaming ? new CamelCasePropertyNamesContractResolver() :  new DefaultContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
            };
        }
    }

    internal class CustomContractResolverJsonSettings<T> : IJsonSettings
    {
        public JsonSerializerSettings GetJSONSettings(bool ignoreNull, bool useCamelCaseNaming = true)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver<T>(useCamelCaseNaming),
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
            };
        }
    }


    internal static class JSONHelper
    {
        public static JsonSerializerSettings GetJSONSettings(bool ignoreNull = true, bool useCamelCaseNaming = true)
        {
            return new CamelCaseContractResolverJsonSettings().GetJSONSettings(ignoreNull, useCamelCaseNaming);
        }

        public static JsonSerializerSettings GetJSONSettings<T>(bool ignoreNull = true, bool useCamelCaseNaming = true)
        {
            return new CustomContractResolverJsonSettings<T>().GetJSONSettings(ignoreNull, useCamelCaseNaming);
        }
    }
}
