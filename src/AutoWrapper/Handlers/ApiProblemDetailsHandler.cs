namespace AutoWrapper.Handlers
{
    using AutoWrapper.Constants;
    using AutoWrapper.Exceptions;
    using AutoWrapper.Extensions;
    using AutoWrapper.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    internal static class ApiProblemDetailsHandler
    {
        private static readonly RouteData _emptyRouteData = new();
        private static readonly ActionDescriptor _emptyActionDescriptor = new();

        public static Task HandleProblemDetailsAsync(HttpContext context, IActionResultExecutor<ObjectResult> executor, object body, Exception? exception, bool isDebug = false)
        {
            var statusCode = TransformStatusCode(context.Response.StatusCode, exception);
            object details = exception == null ? DelegateResponse(body, statusCode) : GetProblemDetails(exception, statusCode, isDebug);

            if (details is ProblemDetails)
            {
                (details as ProblemDetails)!.Instance = context.Request.Path;
            }

            var routeData = context.GetRouteData() ?? _emptyRouteData;

            var actionContext = new ActionContext(context, routeData, _emptyActionDescriptor);

            var result = new ObjectResult(details)
            {
                StatusCode = (details is ProblemDetails problem) ? problem.Status : statusCode,
                DeclaredType = details.GetType()
            };

            result.ContentTypes.Add(ContentMediaTypes.ProblemJSONHttpContentMediaType);
            result.ContentTypes.Add(ContentMediaTypes.ProblemXMLHttpContentMediaType);

            return executor.ExecuteAsync(actionContext, result);

            static int TransformStatusCode(int DefaultStatusCode, Exception? ex)
            {
                return ex switch
                {
                    ApiException ae => ae.StatusCode,
                    UnauthorizedAccessException => Status401Unauthorized,
                    NotImplementedException => Status501NotImplemented,
                    _ => DefaultStatusCode
                };
            }
        }

        private static object DelegateResponse(object? body, int statusCode)
        {
            var content = body ?? string.Empty;
            var (IsEncoded, ParsedText) = content.ToString()!.VerifyBodyContent();
            var result = IsEncoded ? JsonSerializer.Deserialize<dynamic>(ParsedText) : new ApiProblemDetails(statusCode) { Detail = content.ToString() };

            return result ?? string.Empty;
        }

        private static ProblemDetails GetProblemDetails(Exception exception, int statusCode, bool isDebug)
        {
            if (exception is ApiProblemDetailsException problem) { return problem.Problem.Details; }

            var defaultException = new ExceptionFallback(exception);

            if (isDebug)
            {
                return new DebugExceptionetails(defaultException)
                {
                    Status = statusCode
                };
            }

            return new ApiProblemDetails(statusCode) { Detail = defaultException.Exception.Message };
        }

        internal class ErrorDetails
        {
            public string Message { get; set; }
            public string Type { get; set; }
            public string Source { get; set; }
            public string Raw { get; set; }

            public ErrorDetails(ExceptionFallback detail)
            {
                Source = detail.Exception.Source ?? string.Empty;
                Raw = detail.Exception.StackTrace ?? string.Empty;
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

            private static string? GetHelpLink(Exception exception)
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
