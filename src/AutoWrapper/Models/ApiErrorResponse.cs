namespace AutoWrapper.Models
{
    using System.Collections.Generic;

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
}
