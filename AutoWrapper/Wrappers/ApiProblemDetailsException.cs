using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace AutoWrapper.Wrappers
{
    public class ApiProblemDetailsException : Exception
    {
        public ApiProblemDetailsException(int statusCode)
            : this(new ApiProblemDetails(statusCode))
        {
        }

        public ApiProblemDetailsException(int statusCode, string title)
            : this(new ApiProblemDetails(statusCode) { Title = title })
        {
        }

        public ApiProblemDetailsException(ProblemDetails details)
            : base($"{details.Type} : {details.Title}")
        {
            Details = details;
        }

        public ApiProblemDetailsException(ModelStateDictionary modelState, int statusCode = Status422UnprocessableEntity)
        {
            Details = new ValidationProblemDetails(modelState) { Status = statusCode };
        }

        public ProblemDetails Details { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Type    : {Details.Type}");
            stringBuilder.AppendLine($"Title   : {Details.Title}");
            stringBuilder.AppendLine($"Status  : {Details.Status}");
            stringBuilder.AppendLine($"Detail  : {Details.Detail}");
            stringBuilder.AppendLine($"Instance: {Details.Instance}");

            return stringBuilder.ToString();
        }
    }
}
