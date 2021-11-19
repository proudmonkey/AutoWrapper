namespace AutoWrapper.Handlers
{
    using AutoWrapper.Constants;
    using AutoWrapper.Exceptions;
    using AutoWrapper.Models;
    using HelpMate.Core.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    internal class ApiRequestHandlerMember
    {
        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiRequestHandlerMember(AutoWrapperOptions options,
                            ILogger<AutoWrapperMiddleware> logger,
                            JsonSerializerOptions jsonOptions)
        {
            _options = options;
            _logger = logger;
            _jsonOptions = jsonOptions;
        }

        protected static bool IsDefaultSwaggerPath(HttpContext context)
            => context.Request.Path.StartsWithSegments(new PathString("/swagger"));

        protected static bool IsPathMatched(HttpContext context, string path)
        {
            Regex regExclue = new Regex(path);
            return regExclue.IsMatch(context.Request.Path.Value!);
        }

        protected bool IsApiRoute(HttpContext context)
        {
            var fileTypes = new[] { ".js", ".html", ".css" };

            if (_options.IsApiOnly && !fileTypes.Any(context.Request.Path.Value!.Contains))
            {
                return true;
            }

            return context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith));
        }

        protected static async Task WriteFormattedResponseToHttpContextAsync(HttpContext context, int httpStatusCode, string jsonString, JsonDocument? jsonDocument = null)
        {
            context.Response.StatusCode = httpStatusCode;
            context.Response.ContentType = ContentMediaTypes.JSONHttpContentMediaType;
            context.Response.ContentLength = jsonString != null ? Encoding.UTF8.GetByteCount(jsonString!) : 0;

            if (jsonDocument is not null)
            {
                jsonDocument.Dispose();
            }

            await context.Response.WriteAsync(jsonString!);
        }

        protected static ApiErrorResponse WrapErrorResponse(int statusCode, string? message = null) =>
            statusCode switch
            {
                Status204NoContent => new ApiErrorResponse(message ?? ResponseMessage.NoContent, nameof(ResponseMessage.NoContent)),
                Status400BadRequest => new ApiErrorResponse(message ?? ResponseMessage.BadRequest, nameof(ResponseMessage.BadRequest)),
                Status401Unauthorized => new ApiErrorResponse(message ?? ResponseMessage.UnAuthorized, nameof(ResponseMessage.UnAuthorized)),
                Status404NotFound => new ApiErrorResponse(message ?? ResponseMessage.NotFound, nameof(ResponseMessage.NotFound)),
                Status405MethodNotAllowed => new ApiErrorResponse(ResponseMessage.MethodNotAllowed, nameof(ResponseMessage.MethodNotAllowed)),
                Status415UnsupportedMediaType => new ApiErrorResponse(ResponseMessage.MediaTypeNotSupported, nameof(ResponseMessage.MediaTypeNotSupported)),
                Status501NotImplemented => new ApiErrorResponse(ResponseMessage.NotImplemented, nameof(ResponseMessage.NotImplemented)),
                _ => new ApiErrorResponse(ResponseMessage.Unknown, nameof(ResponseMessage.Unknown)),
            };

        protected void LogExceptionWhenEnabled(Exception exception, string message, int statusCode)
        {
            if (_options.EnableExceptionLogging)
            {
                _logger.Log(LogLevel.Error, exception!, $"[{statusCode}]: { message }");
            }
        }

        protected async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
        {
            var ex = exception as ApiException;

            if (ex?.ValidationErrors is not null)
            {
                await HandleValidationErrorAsync(context, ex);
                return;
            }

            if (ex?.CustomErrorModel is not null)
            {
                await HandleCustomErrorAsync(context, ex);
                return;
            }

            await HandleApiErrorAsync(context, ex!);
        }

        protected async Task HandleNotImplementedErrorAsync(HttpContext context, Exception ex)
        {
            var response = new ApiErrorResponse(ResponseMessage.NotImplemented);

            LogExceptionWhenEnabled(ex, ex.Message, Status501NotImplemented);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, Status501NotImplemented, jsonString!);
        }

        protected async Task HandleUnAuthorizedErrorAsync(HttpContext context, Exception ex)
        {
            var response = new ApiErrorResponse(ResponseMessage.UnAuthorized);

            LogExceptionWhenEnabled(ex, ex.Message, Status401Unauthorized);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, Status401Unauthorized, jsonString!);
        }

        protected async Task HandleDefaultErrorAsync(HttpContext context, Exception ex)
        {
            string? details = null;
            string message;

            if (_options.IsDebug)
            {
                message = ex.GetBaseException().Message;
                details = ex.StackTrace;
            }
            else
            {
                message = ResponseMessage.Unhandled;
            }

            var response = new ApiErrorResponse(message, ex.GetType().Name, details);

            LogExceptionWhenEnabled(ex, ex.Message, Status500InternalServerError);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, Status500InternalServerError, jsonString!);
        }

        protected string ConvertToJSONString(int httpStatusCode, object content, string httpMethod)
        {
            var result = content.ToString() ?? string.Empty;
            var statusCode = (!_options.ShowStatusCode) ? null : (int?)httpStatusCode;
            var apiResponse = new ApiResponse($"{httpMethod} {ResponseMessage.Success}", content, statusCode)
            {
                IsError = _options.ShowIsErrorFlagForSuccessfulResponse ? false : null
            };

            var serialized = JsonSerializer.Serialize(apiResponse, _jsonOptions!);

            return result.IsHtml() ? Regex.Unescape(serialized) : serialized;
        }

        protected string ConvertToJSONString(ApiResponse apiResponse)
            => JsonSerializer.Serialize(apiResponse!, _jsonOptions!);

        protected string ConvertToJSONString(object rawJSON) => JsonSerializer.Serialize(rawJSON!, _jsonOptions!);

        protected static ApiResponse WrapSucessfulResponse(ApiResponse apiResponse, string httpMethod)
        {
            apiResponse.Message ??= $"{httpMethod} {ResponseMessage.Success}";
            return apiResponse;
        }

        protected static (bool IsValidated, object ValidatedValue) ValidateSingleValueType(object value)
        {
            var result = value.ToString() ?? string.Empty;
            if (result.IsWholeNumber()) { return (true, result.ToInt64()); }
            if (result.IsDecimalNumber()) { return (true, result.ToDecimal()); }
            if (result.IsBoolean()) { return (true, result.ToBoolean()); }
            if (result.Contains("\"")) { return (true, result.Replace("\"", "")); }

            return (false, value!);
        }

        private async Task HandleValidationErrorAsync(HttpContext context, ApiException ex)
        {
            var response = new ApiErrorResponse(ex.ValidationErrors!);

            LogExceptionWhenEnabled(ex, ex.Message, ex.StatusCode);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, ex.StatusCode, jsonString!);
        }

        private async Task HandleCustomErrorAsync(HttpContext context, ApiException ex)
        {
            var response = new ApiErrorResponse(ex.CustomErrorModel!);

            LogExceptionWhenEnabled(ex, ex.Message, ex.StatusCode);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, ex.StatusCode, jsonString!);
        }

        private async Task HandleApiErrorAsync(HttpContext context, ApiException ex)
        {
            var response = new ApiErrorResponse(ex.Message, ex.ErrorCode ?? ex.GetType().Name);

            LogExceptionWhenEnabled(ex, ex.Message, ex.StatusCode);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, ex.StatusCode, jsonString!);
        }
    }
}
