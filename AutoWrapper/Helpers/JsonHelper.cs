using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoWrapper.Helpers
{
    internal static class JSONHelper
    {
        public static JsonSerializerSettings GetJSONSettings(bool ignoreNull = true, bool useCamelCaseNaming = true)
        {
            return new CamelCaseContractResolverJsonSettings().GetJSONSettings(ignoreNull, useCamelCaseNaming);
        }

        public static (JsonSerializerSettings Settings, Dictionary<string, string> Mappings) GetJSONSettings<T>(bool ignoreNull = true, bool useCamelCaseNaming = true)
        {
            return new CustomContractResolverJsonSettings<T>().GetJSONSettings(ignoreNull, useCamelCaseNaming);
        }

    }
}
