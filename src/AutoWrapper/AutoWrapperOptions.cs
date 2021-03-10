
namespace AutoWrapper
{

    using AutoWrapper.Models;
    using System.Collections.Generic;
    using System.Text.Json;

    public class AutoWrapperOptions
    {
        /// <summary>
        /// Shows the stack trace information in the responseException details.
        /// </summary>
        public bool IsDebug { get; set; } = false;
        /// <summary>
        /// Shows the Api Version attribute in the response.
        /// </summary>
        public bool ShowApiVersion { get; set; } = false;

        /// <summary>
        /// Shows the StatusCode attribute in the response.
        /// </summary>
        public bool ShowStatusCode { get; set; } = false;

        /// <summary>
        /// Shows the IsError attribute in the response.
        /// </summary>
        public bool ShowIsErrorFlagForSuccessfulResponse { get; set; } = false;

        /// <summary>
        /// Use to indicate if the wrapper is used for API project only. 
        /// Set this to false when you want to use the wrapper within an Angular, MVC, React or Blazor projects.
        /// </summary>
        public bool IsApiOnly { get; set; } = true;

        /// <summary>
        /// Tells the wrapper to ignore validation for string that contains HTML
        /// </summary>
        public bool BypassHTMLValidation { get; set; } = false;

        /// <summary>
        /// Set the Api path segment to validate. The default value is '/api'. Only works if IsApiOnly is set to false.
        /// </summary>
        public string WrapWhenApiPathStartsWith { get; set; } = "/api";

        /// <summary>
        /// Tells the wrapper to ignore attributes with null values. Default is true.
        /// </summary>
        public bool IgnoreNullValue { get; set; } = true;

        /// <summary>
        /// Tells the wrapper whether to enable request and response logging. Default is true.
        /// </summary>
        public bool EnableResponseLogging { get; set; } = true;

        /// <summary>
        /// Tells the wrapper whether to enable exception logging. Default is true.
        /// </summary>
        public bool EnableExceptionLogging { get; set; } = true;

        public bool UseApiProblemDetailsException { get; set; } = false;
        public bool LogRequestDataOnException { get; set; } = true;
        public bool IgnoreWrapForOkRequests { get; set; } = false;
        public bool ShouldLogRequestData { get; set; } = true;

        /// <summary>
        /// Tells the wrapper to use the provided JsonNamingPolicy. Default is JsonNamingPolicy.CamelCase.
        /// </summary>
        public JsonNamingPolicy JsonNamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;

        public IEnumerable<ExcludePath>? ExcludePaths { get; set; } = null;
    }
}
