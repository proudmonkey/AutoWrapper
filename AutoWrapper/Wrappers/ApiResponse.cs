using Newtonsoft.Json;
using System.Runtime.Serialization;
using AutoWrapper;

namespace AutoWrapper.Wrappers
{
    public class ApiResponse : ApiResponseBase {

        [JsonConstructor]
        public ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
        {
            
            StatusCode = statusCode;
            Message = message;
            Result = result;
            Version = apiVersion;
            IsError = false;
        }

        public ApiResponse(int statusCode, object apiError)
        {
            StatusCode = statusCode;
            ResponseException = apiError;
            IsError = true;
        }
}
    //public class ApiResponse
    //{
    //    public string Version { get; set; }
    //    public int StatusCode { get; set; }
    //    public string Message { get; set; }
    //    public bool IsError { get; set; }
    //    public object ResponseException { get; set; }

    //    public object Result { get; set; }

    //    [JsonConstructor]
    //    public ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
    //    {
    //        this.StatusCode = statusCode;
    //        this.Message = message;
    //        this.Result = result;
    //        this.Version = apiVersion;
    //        this.IsError = false;
    //    }

    //    public ApiResponse(int statusCode , object apiError)
    //    {
    //        this.StatusCode = statusCode;
    //        this.ResponseException = apiError;
    //        this.IsError = true;
    //    }
    //}
}

