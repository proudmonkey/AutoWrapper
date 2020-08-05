using Microsoft.AspNetCore.Mvc;

namespace AutoWrapper.Models
{
    public class ApiProblemDetailsResponse: ProblemDetails
    {
        public bool IsError { get; set; }
    }
}
