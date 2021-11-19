namespace AutoWrapper.Models.ResponseTypes
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    public class ApiProblemDetailsValidationErrorResponse : ProblemDetails
    {
        public bool IsError { get; set; }
        public string? ErrorID { get; set; }

        public IEnumerable<ValidationError>? ValidationErrors { get; set; }
    }
}
