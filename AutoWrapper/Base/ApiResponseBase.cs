using AutoWrapper.Contracts;

namespace AutoWrapper
{
    public abstract class ApiResponseBase : IApiResponse
    {
        public string Version { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; }

        public bool IsError { get; set; }

        public object ResponseException { get; set; }

        public object Result { get; set; }

        //[JsonConstructor]
        //public ApiResponseBase(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
        //{
        //    this.StatusCode = statusCode;
        //    this.Message = message;
        //    this.Result = result;
        //    this.Version = apiVersion;
        //    this.IsError = false;
        //}

        //public ApiResponseBase(int statusCode, object apiError)
        //{
        //    this.StatusCode = statusCode;
        //    this.ResponseException = apiError;
        //    this.IsError = true;
        //}
    }
}
