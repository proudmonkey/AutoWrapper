using AutoWrapper.Helpers;
using Newtonsoft.Json.Converters;
using System.Text.Json;

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
            return JsonSerializer.Serialize(obj);
        }
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string ToJson(object obj, JsonSerializerOptions jsonOptions)
        {
            var options = new AutoWrapperOptions();
            var newJsonOptions = JSONHelper.GetJSONSettings(options.JsonNamingPolicy, options.IgnoreNullValue);
            return JsonSerializer.Serialize(obj, jsonOptions ?? newJsonOptions);
        }
    }
}
