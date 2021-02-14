using AutoWrapper.Base;
using System.Text.Json;

namespace AutoWrapper
{
    public class AutoWrapperOptions :OptionBase
    {
        public bool UseApiProblemDetailsException { get; set; } = false;
        public bool LogRequestDataOnException { get; set; } = true;
        public bool IgnoreWrapForOkRequests { get; set; } = false;
        public bool ShouldLogRequestData { get; set; } = true;

        /// <summary>
        /// Tells the wrapper to use the provided JsonNamingPolicy. Default is JsonNamingPolicy.CamelCase.
        /// </summary>
        public JsonNamingPolicy JsonNamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;
    }
}
