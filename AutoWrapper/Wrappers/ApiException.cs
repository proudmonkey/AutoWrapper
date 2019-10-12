using System.Collections.Generic;

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
                            int statusCode = 500,
                            string errorCode = "",
                            string refLink = "") :
            base(message)
        {
            //this.IsModelValidatonError = false;
            this.StatusCode = statusCode;
            this.ReferenceErrorCode = errorCode;
            this.ReferenceDocumentLink = refLink;
        }

        public ApiException(object custom, int statusCode = 400)
        {
            this.IsCustomErrorObject = true;
            this.StatusCode = statusCode;
            this.CustomError = custom;
        }

        public ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)
        {
            this.IsModelValidatonError = true;
            this.StatusCode = statusCode;
            this.Errors = errors;
        }

        public ApiException(System.Exception ex, int statusCode = 500) : base(ex.Message)
        {
            this.IsModelValidatonError = false;
            StatusCode = statusCode;
        }
    }
}
