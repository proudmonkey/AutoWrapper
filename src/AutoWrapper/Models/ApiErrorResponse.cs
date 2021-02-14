namespace AutoWrapper.Models
{
    using System.Collections.Generic;

    //public class ApiError
    //{
    //    public object? ExceptionMessage { get; set; }
    //    public string? Details { get; set; }
    //    public string? ReferenceErrorCode { get; set; }
    //    public string? ReferenceDocumentLink { get; set; }
    //    public IEnumerable<ValidationError>? ValidationErrors { get; set; }

    //    public ApiError(object message)
    //    {
    //       ExceptionMessage = message;
    //    }

    //    public ApiError(string message, IEnumerable<ValidationError> validationErrors)
    //    {
    //        ExceptionMessage = message;
    //        ValidationErrors = validationErrors;
    //    }
    //}

    internal class ApiErrorResponse
    {
        public bool? IsError { get; set; } = true;
        public ApiError? Error { get; set; }

        public ApiErrorResponse(string message, string code, string? details = null)
        {
            Error = new ApiError
            {
                Message = message,
                Code = code,
                Details = details ?? null
            };
        }

        public ApiErrorResponse(IEnumerable<ValidationError> validationErrors)
        {
            Error = new ApiError
            {
                Message = "Your request parameters did not validate.",
                Code = "ModelStateError",
                ValidationErrors = validationErrors
            }; 
        }

        public ApiErrorResponse(object errorModel)
        {
            Error = new ApiError
            {
                InnerError = errorModel
            };
        }
    }

    internal class ApiError
    {
        public string? Message { get; set; }
        public string? Code { get; set; }
        public IEnumerable<ValidationError>? ValidationErrors { get; set; }
        public string? Details { get; set; }
        public object? InnerError { get; set; }
    }

}
