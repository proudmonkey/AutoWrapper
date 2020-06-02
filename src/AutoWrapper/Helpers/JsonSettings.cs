using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace AutoWrapper.Helpers
{
    public class CamelCaseContractResolverJsonSettings
    {
        public JsonSerializerSettings GetJSONSettings(bool ignoreNull, ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore, bool useCamelCaseNaming = true)
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = useCamelCaseNaming ? new CamelCasePropertyNamesContractResolver() :  new DefaultContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
                ReferenceLoopHandling = referenceLoopHandling
            };
        }
    }

    public class CustomContractResolverJsonSettings<T>
    {
        public (JsonSerializerSettings Settings, Dictionary<string, string> Mappings) GetJSONSettings(bool ignoreNull, ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore, bool useCamelCaseNaming = true)
        {
            var resolver = new CustomContractResolver<T>(useCamelCaseNaming);
            var propMappings = resolver._propertyMappings;

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = resolver,
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
                ReferenceLoopHandling = referenceLoopHandling
            };

            return (settings, propMappings);

        }
    }


  
}
