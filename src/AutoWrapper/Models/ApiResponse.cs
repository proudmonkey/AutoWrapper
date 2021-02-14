namespace AutoWrapper.Models
{
    using static Microsoft.AspNetCore.Http.StatusCodes;
    public class ApiResponse
    {
        public bool? IsError { get; set; } = false;
        public string? Version { get; set; }
  
        public int? StatusCode { get; set; }

        public string Message { get; set; } = null!;

        //public object? ResponseException { get; set; }

        public object? Result { get; set; }

        public ApiResponse(string message, object? result = null, int statusCode = Status200OK)
        {
            StatusCode = statusCode == 0 ? null : (int?)statusCode;
            Message = message;
            Result = result;
        }
        public ApiResponse(object result, int statusCode = 200)
        {
            StatusCode = statusCode == 0 ? null : (int?)statusCode;
            Result = result;
        }

        //public ApiResponse(int statusCode, object apiError)
        //{
        //    StatusCode = statusCode == 0 ? null : (int?)statusCode;
        //    ResponseException = apiError;
        //    IsError = true;
        //}

        public ApiResponse() { }
    }
}


