using AutoWrapper.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            var jsonSettings = JSONHelper.GetJSONSettings(options.IgnoreNullValue, options.ReferenceLoopHandling, options.UseCamelCaseNamingStrategy);
            return JsonConvert.SerializeObject(obj, settings ?? jsonSettings);
        }
    }
}
