using AutoWrapper.Extensions;
using AutoWrapper.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace AutoWrapper.Wrappers
{
    internal class ApiProblemDetailsMember
    {
        private static readonly RouteData _emptyRouteData = new RouteData();
        private static readonly ActionDescriptor _emptyActionDescriptor = new ActionDescriptor();
        public Task WriteProblemDetailsAsync(HttpContext context, IActionResultExecutor<ObjectResult> executor, object body, Exception exception, bool isDebug = false)
        {
            var statusCode = context.Response.StatusCode;
            object details = exception == null ? DelegateResponse(body, statusCode) : GetProblemDetails(exception, isDebug);

            if (details is ProblemDetails) { (details as ProblemDetails).Instance = context.Request.Path; }

            var routeData = context.GetRouteData() ?? _emptyRouteData;

            var actionContext = new ActionContext(context, routeData, _emptyActionDescriptor);

            var result = new ObjectResult(details)
            {
                StatusCode = (details is ProblemDetails problem) ? problem.Status : statusCode,
                DeclaredType = details.GetType()
            };

            result.ContentTypes.Add(TypeIdentifier.ProblemJSONHttpContentMediaType);
            result.ContentTypes.Add(TypeIdentifier.ProblemXMLHttpContentMediaType);

            return executor.ExecuteAsync(actionContext, result);
        }

        private object DelegateResponse(object body, int statusCode)
        {
            var content = body ?? string.Empty;
            var (IsEncoded, ParsedText) = content.ToString().VerifyBodyContent();
            var result = IsEncoded ? JsonConvert.DeserializeObject<dynamic>(ParsedText) : new ApiProblemDetails(statusCode) {  Detail = content.ToString() } ;

            return result;
        }
        private ProblemDetails GetProblemDetails(Exception exception, bool isDebug)
        {
            if (exception is ApiProblemDetailsException problem){ return problem.Problem.Details; }

            var defaultException = new ExceptionFallback(exception);

            if (isDebug) { return new DebugExceptionetails(defaultException); }

            return new ApiProblemDetails((int)defaultException.Status) { Detail = defaultException.Exception.Message };
        }

        internal class ErrorDetails
        {
            public string Message { get; set; }
            public string Type { get; set; }
            public string Source { get; set; }
            public string Raw { get; set; }

            public ErrorDetails(ExceptionFallback detail)
            {
                Source = detail.Exception.Source;
                Raw = detail.Exception.StackTrace;
                Message = detail.Exception.Message;
                Type = detail.Exception.GetType().Name;
            }
        }

        internal class ExceptionFallback : ApiProblemDetails
        {
            public ExceptionFallback(Exception exception) : this(exception, Status500InternalServerError)
            {
                Detail = exception.Message;
            }

            public ExceptionFallback(Exception exception, int statusCode) : base(statusCode)
            {
                Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            }

            public Exception Exception { get; }
        }

        internal class DebugExceptionetails : ApiProblemDetails
        {
            public DebugExceptionetails(ExceptionFallback problem)
                : base(problem.Status ?? Status500InternalServerError)
            {
                Detail = problem.Detail ?? problem.Exception.Message;
                Title = problem.Title ?? problem.Exception.GetType().Name;
                Instance = problem.Instance ?? GetHelpLink(problem.Exception);

                if (!string.IsNullOrEmpty(problem.Type))
                {
                    Type = problem.Type;
                }

                Errors = new ErrorDetails(problem);
            }

            private static string GetHelpLink(Exception exception)
            {
                var link = exception.HelpLink;

                if (string.IsNullOrEmpty(link))
                {
                    return null;
                }

                if (Uri.TryCreate(link, UriKind.Absolute, out var result))
                {
                    return result.ToString();
                }

                return null;
            }

        }
    }
}
