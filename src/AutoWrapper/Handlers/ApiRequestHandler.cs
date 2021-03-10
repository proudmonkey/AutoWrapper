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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    internal class ApiRequestHandler : ApiRequestHandlerMember
    {
        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        //private readonly ApiRequestHandlerMember _handlerMember;

        public ApiRequestHandler(AutoWrapperOptions options,
                                    ILogger<AutoWrapperMiddleware> logger,
                                    JsonSerializerOptions jsonOptions)
            : base(options, logger, jsonOptions)
        {
            _options = options;
            _logger = logger;
            _jsonOptions = jsonOptions;
        }
   
        public async Task<string?> GetRequestBodyAsync(HttpRequest request)
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

        //public async Task<string> ReadResponseBodyStreamAsync(Stream bodyStream)
        //{
        //    bodyStream.Seek(0, SeekOrigin.Begin);
        //    var responseBody = await new StreamReader(bodyStream!).ReadToEndAsync();
        //    bodyStream.Seek(0, SeekOrigin.Begin);

        //    var (IsEncoded, ParsedText) = responseBody.VerifyBodyContent();

        //    return IsEncoded ? ParsedText : responseBody;
        //}

        public async Task<(bool IsEncoded, string ParsedText, JsonDocument? JsonDoc)> ReadResponseBodyStreamAsync(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(bodyStream!).ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);

            return responseBody.VerifyAndParseBodyContentToJson();
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
                    await HandleExceptionAsync(context, exception);
                    break;
                case UnauthorizedAccessException:
                    await HandleUnAuthorizedErrorAsync(context, exception);
                    break;
                default:
                    await HandleDefaultErrorAsync(context, exception);
                    break;
            }
        }

        public async Task HandleUnsuccessfulRequestAsync(HttpContext context, object body, int httpStatusCode)     
        {
            var isJsonShape = body.ToString()!.IsValidJson();
            var bodyText = body.ToString()!;
            var message = isJsonShape && !string.IsNullOrEmpty(bodyText) ?  null : bodyText;
            
            var response = WrapErrorResponse(httpStatusCode!, message);
            var jsonString = JsonSerializer.Serialize(response, _jsonOptions!);
            await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, jsonString!);
        }

        public async Task HandleSuccessfulRequestAsync(HttpContext context, string bodyAsText, int httpStatusCode, JsonDocument? jsonDocument)
        {
            string wrappedJsonString;

            if (jsonDocument is null)
            {
                var (IsValidated, ValidatedValue) = ValidateSingleValueType(bodyAsText);
                var result = IsValidated ? ValidatedValue : bodyAsText;
                wrappedJsonString = ConvertToJSONString(httpStatusCode!, result!, context.Request.Method);

                await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
                return;
            }
            //var jsonString = !body.ToString().IsValidJson() ? ConvertToJSONString(body!) : body.ToString();

            //using var jsonDocument = JsonDocument.Parse(bodyAsText);
            var root = jsonDocument.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {

                if (IsApiResponseJsonShape(root))
                {
                    //var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonString!, _jsonOptions);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(root.GetRawText(),_jsonOptions);

                    httpStatusCode = apiResponse?.StatusCode ?? context.Response.StatusCode;

                    wrappedJsonString = ConvertToJSONString(WrapSucessfulResponse(apiResponse!, context.Request.Method));
                    await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
                    return;
                }

                wrappedJsonString = ConvertToJSONString(httpStatusCode!, root, context.Request.Method);
                await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
                return;
            }

            //var (IsValidated, ValidatedValue) = ValidateSingleValueType(root);
            //var result = IsValidated ? ValidatedValue : root;
            //wrappedJsonString = ConvertToJSONString(httpStatusCode!, result!, context.Request.Method);

            //await WriteFormattedResponseToHttpContextAsync(context!, httpStatusCode!, wrappedJsonString!);
        }

        public async Task HandleNotApiRequestAsync(HttpContext context)
        {
            string configErrorText = ResponseMessage.NotApiOnly;
            context.Response.ContentLength = configErrorText != null ? Encoding.UTF8.GetByteCount(configErrorText!) : 0;
            await context.Response.WriteAsync(configErrorText!);
        }

        public bool ShouldIgnoreRequest(HttpContext context, IEnumerable<ExcludePath>? excludePaths)
        {

            if (IsDefaultSwaggerPath(context))
            {
                return true;
            }

            if (!IsApiRoute(context))
            {
                return true;
            }

            if (excludePaths is null || !excludePaths.Any())
            {
                return false;
            }

            return excludePaths.Any(x =>
            {
                var path = x.Path ?? string.Empty;

                return x.ExcludeMode switch
                {
                    ExcludeMode.Strict => context.Request.Path.Value == x.Path,
                    ExcludeMode.StartsWith => context.Request.Path.StartsWithSegments(new PathString(x.Path)),
                    ExcludeMode.Regex => IsPathMatched(context, path),
                    _ => false
                };
            });
        }



        public async Task WrapIgnoreAsync(HttpContext context, object body)
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

        public static bool IsRequestSuccessful(int statusCode)
            => statusCode >= 200 && statusCode < 400;

    }
}
