namespace AutoWrapper.Handlers
{
    using AutoWrapper.Configurations;
    using AutoWrapper.Constants;
    using AutoWrapper.Exceptions;
    using AutoWrapper.Extensions;
    using AutoWrapper.Models;
    using HelpMate.Core;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    internal class ApiRequestHandler
    {
        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiRequestHandler(AutoWrapperOptions options,
                                    ILogger<AutoWrapperMiddleware> logger,
                                    JsonSerializerOptions jsonOptions)
        {
            _options = options;
            _logger = logger;
            _jsonOptions = jsonOptions;
        }

        public static async Task<string?> GetRequestBodyAsync(HttpRequest request)
        {
            var httpMethodsWithRequestBody = new[] { "POST", "PUT", "PATCH" };
            var hasRequestBody = httpMethodsWithRequestBody.Any(x => x.Equals(request.Method.ToUpper()));
            string? requestBody = null;

            if (hasRequestBody)
            {
                request.EnableBuffering();

                using var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream!);
                requestBody = Encoding.UTF8.GetString(memoryStream.ToArray());
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            return requestBody;
        }

        public async Task<string> ReadResponseBodyStreamAsync(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(bodyStream!).ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);

            var (IsEncoded, ParsedText) = responseBody.VerifyBodyContent();

            return IsEncoded ? ParsedText : responseBody;
        }

        public async Task RevertResponseBodyStreamAsync(Stream bodyStream, Stream orginalBodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            await bodyStream.CopyToAsync(orginalBodyStream!);
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            switch (exception)
            {
                case ApiException:
                    {
                        var ex = exception as ApiException;

                        if (ex?.ValidationErrors is not null)
                        {
                            await HandleValidationErrorAsync(context, ex);
                            break;
                        }

                        if (ex?.CustomErrorModel is not null)
                        {
                            await HandleCustomErrorAsync(context, ex);
                            break;
                        }

                        await HandleApiErrorAsync(context, ex!);
                        break;
                    }

                case UnauthorizedAccessException:
                    {
                        await HandleUnAuthorizedErrorAsync(context, exception);
                        break;
                    }
                default:
                    {
                        await HandleDefaultErrorAsync(context, exception);
                        break;
                    }
            }
        }

        public async Task HandleUnsuccessfulRequestAsync(HttpContext context, object body, int statusCode)     
        {
            var isJsonShape = body.ToString()!.IsValidJson();
            var bodyText = body.ToString()!;
            var message = isJsonShape && !string.IsNullOrEmpty(bodyText) ?  null : bodyText;
            
            var response = WrapUnsucessfulError(statusCode!, message);
            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);
            await WriteFormattedResponseToHttpContextAsync(context!, statusCode!, jsonString!);
        }

        public async Task HandleSuccessfulRequestAsync(HttpContext context, object body, int httpStatusCode)
        {
            var jsonString = !body.ToString().IsValidJson() ? ConvertToJSONString(body!) : body.ToString();

            using var jsonDocument = JsonDocument.Parse(jsonString!);
            var root = jsonDocument.RootElement;

            string wrappedJsonString;

            if (root.ValueKind == JsonValueKind.Object)
            {
                var hasExpectedShape = ValidateExpectedJsonShape(root);
             
                if (hasExpectedShape)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonString!);
                    httpStatusCode = (int)apiResponse?.StatusCode!;

                    wrappedJsonString = ConvertToJSONString(GetSucessResponse(apiResponse!, context.Request.Method));
                    await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
                    return;
                }

                wrappedJsonString = ConvertToJSONString(httpStatusCode!, root, context.Request.Method);
                await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
                return;
            }

            var (IsValidated, ValidatedValue) = ValidateSingleValueType(root);
            var result = IsValidated ? ValidatedValue : root;
            wrappedJsonString = ConvertToJSONString(httpStatusCode!, result!, context.Request.Method);

            await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
        }

        public async Task HandleNotApiRequestAsync(HttpContext context)
        {
            string configErrorText = ResponseMessage.NotApiOnly;
            context.Response.ContentLength = configErrorText != null ? Encoding.UTF8.GetByteCount(configErrorText!) : 0;
            await context.Response.WriteAsync(configErrorText!);
        }

        public static bool IsSwagger(HttpContext context)
            => context.Request.Path.StartsWithSegments(new PathString("/swagger"));

        public bool IsApi(HttpContext context)
        {
            if (_options.IsApiOnly && !context.Request.Path.Value!.Contains(".js") && !context.Request.Path.Value.Contains(".css"))
            {
                return true;
            }

            return context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith));
        }

        public static async Task WrapIgnoreAsync(HttpContext context, object body)
        {
            var bodyText = body.ToString();
            context.Response.ContentLength = bodyText != null ? Encoding.UTF8.GetByteCount(bodyText!) : 0;
            await context.Response.WriteAsync(bodyText!);
        }

        public async Task HandleProblemDetailsExceptionAsync(HttpContext context, IActionResultExecutor<ObjectResult> executor, object? body, Exception? exception = null)
        {
            await ApiProblemDetailsHandler.HandleProblemDetailsAsync(context!, executor!, body!, exception!, _options.IsDebug);

            if (_options.EnableExceptionLogging && exception != null)
            {
                _logger.Log(LogLevel.Error, exception!, $"[{context.Response.StatusCode}]: { exception.GetBaseException().Message }");
            }
        }

        private void TryLogException(Exception exception, string message, int statusCode)
        {
            if (_options.EnableExceptionLogging)
            {
                _logger.Log(LogLevel.Error, exception!, $"[{statusCode}]: { message }");
            }
        }

        public static bool IsRequestSuccessful(int statusCode)
            => statusCode >= 200 && statusCode < 400;

        private async Task HandleValidationErrorAsync(HttpContext context, ApiException ex)
        {
            var response = new ApiErrorResponse(ex.ValidationErrors!);

            TryLogException(ex, ex.Message, ex.StatusCode);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, ex.StatusCode, jsonString!);
        }

        private async Task HandleCustomErrorAsync(HttpContext context, ApiException ex)
        {
            var response = new ApiErrorResponse(ex.CustomErrorModel!);

            TryLogException(ex, ex.Message, ex.StatusCode);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, ex.StatusCode, jsonString!);
        }

        private async Task HandleApiErrorAsync(HttpContext context, ApiException ex)
        {
            var response = new ApiErrorResponse(ex.Message, ex.ErrorCode ?? ex.GetType().Name);

            TryLogException(ex, ex.Message, ex.StatusCode);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, ex.StatusCode, jsonString!);
        }

        private async Task HandleUnAuthorizedErrorAsync(HttpContext context, Exception ex)
        {
            var response = new ApiErrorResponse(ResponseMessage.UnAuthorized);

            TryLogException(ex, ex.Message, Status401Unauthorized);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, Status401Unauthorized, jsonString!);
        }

        private async Task HandleDefaultErrorAsync(HttpContext context, Exception ex)
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

            TryLogException(ex, ex.Message, Status500InternalServerError);

            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);

            await WriteFormattedResponseToHttpContextAsync(context!, Status500InternalServerError, jsonString!);
        }

        private static async Task WriteFormattedResponseToHttpContextAsync(HttpContext context, int httpStatusCode, string jsonString)
        {
            context.Response.StatusCode = httpStatusCode;
            context.Response.ContentType = ContentMediaTypes.JSONHttpContentMediaType;
            context.Response.ContentLength = jsonString != null ? Encoding.UTF8.GetByteCount(jsonString!) : 0;
            await context.Response.WriteAsync(jsonString!);
        }

        private string ConvertToJSONString(int httpStatusCode, object content, string httpMethod)
        {
            var apiResponse = new ApiResponse($"{httpMethod} {ResponseMessage.Success}", content!, !_options.ShowStatusCode ? 0 : httpStatusCode);
            return JsonSerializer.Serialize(apiResponse!, _jsonOptions!);
        }

        private string ConvertToJSONString(ApiResponse apiResponse)
            => JsonSerializer.Serialize(apiResponse!, _jsonOptions!);
        
        private string ConvertToJSONString(object rawJSON) => JsonSerializer.Serialize(rawJSON!, _jsonOptions!);

        private static ApiErrorResponse WrapUnsucessfulError(int statusCode, string? message = null) =>
            statusCode switch
            {
                Status204NoContent => new ApiErrorResponse(message ?? ResponseMessage.NoContent, nameof(ResponseMessage.NoContent)),
                Status400BadRequest => new ApiErrorResponse(message ?? ResponseMessage.BadRequest, nameof(ResponseMessage.BadRequest)),
                Status401Unauthorized => new ApiErrorResponse(message ?? ResponseMessage.UnAuthorized, nameof(ResponseMessage.UnAuthorized)),
                Status404NotFound => new ApiErrorResponse(message ?? ResponseMessage.NotFound, nameof(ResponseMessage.NotFound)),
                Status405MethodNotAllowed => new ApiErrorResponse(ResponseMessage.MethodNotAllowed, nameof(ResponseMessage.MethodNotAllowed)),
                Status415UnsupportedMediaType => new ApiErrorResponse(ResponseMessage.MediaTypeNotSupported, nameof(ResponseMessage.MediaTypeNotSupported)),
                _ => new ApiErrorResponse(ResponseMessage.Unknown, nameof(ResponseMessage.Unknown))
            };

        private static ApiResponse GetSucessResponse(ApiResponse apiResponse, string httpMethod)
        {
            apiResponse.Message ??= $"{httpMethod} {ResponseMessage.Success}";
            return apiResponse;
        }

        private string? GetApiVersion() => _options.ApiVersion;

        private static (bool IsValidated, object ValidatedValue) ValidateSingleValueType(object value)
        {
            var result = value.ToString();
            if (result.IsWholeNumber()) { return (true, result.ToInt64()); }
            if (result.IsDecimalNumber()) { return (true, result.ToDecimal()); }
            if (result.IsBoolean()) { return (true, result.ToBoolean()); }

            return (false, value!);
        }

        private static bool ValidateExpectedJsonShape(JsonElement root)
            => root.TryGetProperty("Message", out _) 
               && root.TryGetProperty("Result", out _)
               && root.TryGetProperty("ResponseException", out _);

    }
}
