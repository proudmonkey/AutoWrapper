using Newtonsoft.Json;

namespace AutoWrapper.Wrappers
{
    public class ApiResponse
    {
        public string Version { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StatusCode { get; set; }

        public string Message { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? IsError { get; set; } 

        public object ResponseException { get; set; }

        public object Result { get; set; }

        [JsonConstructor]
        public ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
            Version = apiVersion;
        }
        public ApiResponse(object result, int statusCode = 200)
        {
            StatusCode = statusCode;
            Result = result;
        }

        public ApiResponse(int statusCode, object apiError)
        {
            StatusCode = statusCode;
            ResponseException = apiError;
            IsError = true;
        }

        public ApiResponse() { }
    }
}


