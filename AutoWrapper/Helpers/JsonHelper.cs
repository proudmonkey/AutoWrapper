using AutoWrapper.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

        public static bool HasProperty(dynamic obj, string name)
        {
            if (obj is JObject) return ((JObject)obj).ContainsKey(name);
            return obj.GetType().GetProperty(name) != null;
        }
        public static JToken RemoveEmptyChildren(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }

                    if(!child.IsNullOrEmpty())
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }

                    if (!child.IsNullOrEmpty())
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        public static bool IsEmpty(JToken token)
        {
            return (token.Type == JTokenType.Null);
        }
    }
}
