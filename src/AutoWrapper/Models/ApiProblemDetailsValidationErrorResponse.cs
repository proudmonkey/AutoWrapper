using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AutoWrapper.Models
{
    public class ApiProblemDetailsValidationErrorResponse: ProblemDetails
    {
        public bool IsError { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }
    }
}
