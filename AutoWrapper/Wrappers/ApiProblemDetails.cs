using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using static AutoWrapper.Wrappers.ApiProblemDetailsMember;

namespace AutoWrapper.Wrappers
{

    internal class ApiProblemDetails: ProblemDetails
    {
        public ApiProblemDetails(int statusCode)
        {
            Status = statusCode;
            Type = $"https://httpstatuses.com/{statusCode}";
            Title = ReasonPhrases.GetReasonPhrase(statusCode);
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ErrorDetails Errors { get; set; }
    }
}
