using Microsoft.AspNetCore.Mvc;

namespace AutoWrapper.Models.ResponseTypes
{
    public class ApiProblemDetailsResponse : ProblemDetails
    {
        public bool IsError { get; set; }
        public string? ErrorID { get; set; }

    }
}
