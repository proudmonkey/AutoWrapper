namespace AutoWrapper.Models
{
    using AutoWrapper.Interface;
    public class ApiResponse : IApiResponse
    {
        public bool? IsError { get; set; }

        public int? StatusCode { get; set; }

        public string? Message { get; set; } 

        public object? Result { get; set; }

        public ApiResponse(string message, object? result = null, int? statusCode = null)
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
        }
        public ApiResponse(object result, int statusCode = 200)
        {
            StatusCode = statusCode == 0 ? null : statusCode;
            Result = result;
        }

        public ApiResponse() { }
    }
}


