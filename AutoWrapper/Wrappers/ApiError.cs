using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoWrapper.Wrappers
{
    public class ApiError
    {
        public string ExceptionMessage { get; set; }
        public string Details { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }
        //public object Custom { get; set; } = null;
        public ApiError(string message)
        {
            this.ExceptionMessage = message;
        }

        public ApiError(string message, IEnumerable<ValidationError> validationErrors)
        {
            this.ExceptionMessage = message;
            this.ValidationErrors = validationErrors;
        }

        //public ApiError(string message, object custom)
        //{
        //    this.ExceptionMessage = message;
        //    this.Custom = custom;
        //}
    }
}
