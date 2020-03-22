using System.Collections.Generic;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace AutoWrapper.Wrappers
{
    public class ApiException : System.Exception
    {
        public int StatusCode { get; set; }
        public bool IsModelValidatonError { get; set; } = false;
        public IEnumerable<ValidationError> Errors { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }
        public object CustomError { get; set; }
        public bool IsCustomErrorObject { get; set; } = false;

        public ApiException(string message,
                            int statusCode = Status400BadRequest,
                            string errorCode = default,
                            string refLink = default) :
            base(message)
        {
            StatusCode = statusCode;
            ReferenceErrorCode = errorCode;
            ReferenceDocumentLink = refLink;
        }

        public ApiException(object customError, int statusCode = Status400BadRequest)
        {
            IsCustomErrorObject = true;
            StatusCode = statusCode;
            CustomError = customError;
        }

        public ApiException(IEnumerable<ValidationError> errors, int statusCode = Status400BadRequest)
        {
            IsModelValidatonError = true;
            StatusCode = statusCode;
            Errors = errors;
        }

        public ApiException(System.Exception ex, int statusCode = Status500InternalServerError) : base(ex.Message)
        {
            StatusCode = statusCode;
        }
    }
}
