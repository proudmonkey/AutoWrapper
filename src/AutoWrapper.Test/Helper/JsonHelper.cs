using AutoWrapper.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AutoWrapper.Test.Helper
{
    public static class JsonHelper
    {
        /// <summary>
        ///  
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string ToJson(object obj, JsonSerializerSettings settings)
        {
            var options = new AutoWrapperOptions();
            var jsonSettings = GetJSONSettings(options.IgnoreNullValue, options.ReferenceLoopHandling, options.UseCamelCaseNamingStrategy);
            return JsonConvert.SerializeObject(obj, settings ?? jsonSettings);
        }

        public static JsonSerializerSettings GetJSONSettings(bool ignoreNull = true, ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore, bool useCamelCaseNaming = true)
        {
            return new CamelCaseContractResolverJsonSettings().GetJSONSettings(ignoreNull, referenceLoopHandling, useCamelCaseNaming);
        }

        public static (JsonSerializerSettings Settings, Dictionary<string, string> Mappings) GetJSONSettings<T>(bool ignoreNull = true, ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore, bool useCamelCaseNaming = true)
        {
            return new CustomContractResolverJsonSettings<T>().GetJSONSettings(ignoreNull, referenceLoopHandling, useCamelCaseNaming);
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

                    if (!child.IsNullOrEmpty())
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

    }
}
