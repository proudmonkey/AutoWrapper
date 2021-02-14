namespace AutoWrapper.Models
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using static AutoWrapper.Handlers.ApiProblemDetailsHandler;

    internal class ApiProblemDetails: ProblemDetails
    {
        public ApiProblemDetails(int statusCode)
        {
            IsError = true;
            Status = statusCode;
            Type = $"https://httpstatuses.com/{statusCode}";
            Title = ReasonPhrases.GetReasonPhrase(statusCode);
        }

        public ApiProblemDetails(ProblemDetails details)
        {
            IsError = true;
            Details = details;
        }

        public bool IsError { get; set; }

        public ErrorDetails? Errors { get; set; }

        public IEnumerable<ValidationError>? ValidationErrors { get; set; }

        [JsonIgnore]
        public ProblemDetails Details { get; set; } = null!;
    }
}
