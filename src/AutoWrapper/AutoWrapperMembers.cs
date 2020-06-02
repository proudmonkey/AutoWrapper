using AutoWrapper.Extensions;
using AutoWrapper.Helpers;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace AutoWrapper
{
    internal class AutoWrapperMembers
    {

        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        private readonly JsonSerializerSettings _jsonSettings;
        public readonly Dictionary<string, string> _propertyMappings;
        private readonly bool _hasSchemaForMappping;
        public AutoWrapperMembers(AutoWrapperOptions options, 
                                    ILogger<AutoWrapperMiddleware> logger, 
                                    JsonSerializerSettings jsonSettings,
                                    Dictionary<string, string> propertyMappings = null, 
                                    bool hasSchemaForMappping = false)
        {
            _options = options;
            _logger = logger;
            _jsonSettings = jsonSettings;
            _propertyMappings = propertyMappings;
            _hasSchemaForMappping = hasSchemaForMappping;
        }

        public async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            var httpMethodsWithRequestBody = new[] { "POST", "PUT", "PATCH" };
            var hasRequestBody = httpMethodsWithRequestBody.Any(x => x.Equals(request.Method.ToUpper()));
            string requestBody = default;

            if (hasRequestBody)
            {
                request.EnableBuffering();

                using var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream);
                requestBody = Encoding.UTF8.GetString(memoryStream.ToArray());
                request.Body.Seek(0, SeekOrigin.Begin);
            }
            return requestBody;
        }

        public async Task<string> ReadResponseBodyStreamAsync(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(bodyStream).ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);

            var (IsEncoded, ParsedText) = responseBody.VerifyBodyContent();

            return IsEncoded ? ParsedText : responseBody;
        }

        public async Task RevertResponseBodyStreamAsync(Stream bodyStream, Stream orginalBodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            await bodyStream.CopyToAsync(orginalBodyStream);
        }

        public async Task HandleExceptionAsync(HttpContext context, System.Exception exception)
        {
            if (_options.UseCustomExceptionFormat)
            {
                await WriteFormattedResponseToHttpContextAsync(context, context.Response.StatusCode, exception.GetBaseException().Message);
                return;
            }

            string exceptionMessage = default;
            object apiError;
            int httpStatusCode;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                if (ex.IsModelValidatonError)
                {
                    apiError = new ApiError(ResponseMessage.ValidationError, ex.Errors) { ReferenceErrorCode = ex.ReferenceErrorCode, ReferenceDocumentLink = ex.ReferenceDocumentLink };
                }
                else if (ex.IsCustomErrorObject)
                {
                    apiError = ex.CustomError;
                }
                else
                {
                    apiError = new ApiError(ex.Message) { ReferenceErrorCode = ex.ReferenceErrorCode, ReferenceDocumentLink = ex.ReferenceDocumentLink };
                }

                httpStatusCode = ex.StatusCode;
            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError(ResponseMessage.UnAuthorized);
                httpStatusCode = Status401Unauthorized;
            }
            else
            {
                string stackTrace = null;

                if (_options.IsDebug)
                {
                    exceptionMessage = $"{ exceptionMessage } { exception.GetBaseException().Message }";
                    stackTrace = exception.StackTrace;
                }
                else
                {
                    exceptionMessage = ResponseMessage.Unhandled;
                }

                apiError = new ApiError(exceptionMessage) { Details = stackTrace };
                httpStatusCode = Status500InternalServerError;
            }


            if (_options.EnableExceptionLogging) {
                var errorMessage = apiError is ApiError ? ((ApiError)apiError).ExceptionMessage : ResponseMessage.Exception;
                _logger.Log(LogLevel.Error, exception, $"[{httpStatusCode}]: { errorMessage }");
            }

            var jsonString = ConvertToJSONString(GetErrorResponse(httpStatusCode, apiError));

            await WriteFormattedResponseToHttpContextAsync(context, httpStatusCode, jsonString);
        }

        public async Task HandleUnsuccessfulRequestAsync(HttpContext context, object body, int httpStatusCode)
        {
            var (IsEncoded, ParsedText) = body.ToString().VerifyBodyContent();

            if (IsEncoded && _options.UseCustomExceptionFormat)
            {
                await WriteFormattedResponseToHttpContextAsync(context, httpStatusCode, body.ToString());
                return;
            }

            var bodyText = IsEncoded ? JsonConvert.DeserializeObject<dynamic>(ParsedText) : body.ToString();
            ApiError apiError = !string.IsNullOrEmpty(body.ToString()) ? new ApiError(bodyText) : WrapUnsucessfulError(httpStatusCode);

            var jsonString = ConvertToJSONString(GetErrorResponse(httpStatusCode, apiError));
            await WriteFormattedResponseToHttpContextAsync(context, httpStatusCode, jsonString);
        }

        public async Task HandleSuccessfulRequestAsync(HttpContext context, object body, int httpStatusCode)
        {
            var bodyText = !body.ToString().IsValidJson() ? ConvertToJSONString(body) : body.ToString();

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);

            Type type = bodyContent?.GetType();

            string jsonString;
            if (type.Equals(typeof(JObject)))
            {
                ApiResponse apiResponse = new ApiResponse();

                if (_options.UseCustomSchema)
                {
                    var formatJson = _options.IgnoreNullValue ? JSONHelper.RemoveEmptyChildren(bodyContent) : bodyContent;
                    await WriteFormattedResponseToHttpContextAsync(context, httpStatusCode, JsonConvert.SerializeObject(formatJson));
                }
                else
                {
                    if (_hasSchemaForMappping && (_propertyMappings.Count == 0 || _propertyMappings == null))
                        throw new ApiException(ResponseMessage.NoMappingFound);
                    else
                        apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);
                }

                if (apiResponse.StatusCode == 0 && apiResponse.Result == null && apiResponse.ResponseException == null)
                    jsonString = ConvertToJSONString(httpStatusCode, bodyContent, context.Request.Method);
                else if ((apiResponse.StatusCode != httpStatusCode || apiResponse.Result != null) ||
                        (apiResponse.StatusCode == httpStatusCode && apiResponse.Result == null))
                {
                    httpStatusCode = apiResponse.StatusCode; // in case response is not 200 (e.g 201)
                    jsonString = ConvertToJSONString(GetSucessResponse(apiResponse, context.Request.Method));

                }
                else
                    jsonString = ConvertToJSONString(httpStatusCode, bodyContent, context.Request.Method);
            }
            else
            {
                var validated = ValidateSingleValueType(bodyContent);
                object result = validated.Item1 ? validated.Item2 : bodyContent;
                jsonString = ConvertToJSONString(httpStatusCode, result, context.Request.Method);
            }

            await WriteFormattedResponseToHttpContextAsync(context, httpStatusCode, jsonString);
        }

        public async Task HandleNotApiRequestAsync(HttpContext context)
        {
            string configErrorText = ResponseMessage.NotApiOnly;
            context.Response.ContentLength = configErrorText != null ? Encoding.UTF8.GetByteCount(configErrorText) : 0;
            await context.Response.WriteAsync(configErrorText);
        }

        public bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(new PathString("/swagger"));
        }

        public bool IsApi(HttpContext context)
        {
            if (_options.IsApiOnly && !context.Request.Path.Value.Contains(".js") && !context.Request.Path.Value.Contains(".css"))
                return true;

            return context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith));
        }

        public async Task WrapIgnoreAsync(HttpContext context, object body)
        {
            var bodyText = body.ToString();
            context.Response.ContentLength = bodyText != null ? Encoding.UTF8.GetByteCount(bodyText) : 0;
            await context.Response.WriteAsync(bodyText);
        }

        public async Task HandleProblemDetailsExceptionAsync(HttpContext context, IActionResultExecutor<ObjectResult> executor, object body, Exception exception = null)
        {
            await new ApiProblemDetailsMember().WriteProblemDetails(context, executor, body, exception, _options.IsDebug);

            if (_options.EnableExceptionLogging && exception != null)
            {
                _logger.Log(LogLevel.Error, exception, $"[{context.Response.StatusCode}]: { exception.GetBaseException().Message }");
            }
        }

        public bool IsRequestSuccessful(int statusCode)
        {
            return (statusCode >= 200 && statusCode < 400);
        }

        #region Private Members

        private async Task WriteFormattedResponseToHttpContextAsync(HttpContext context, int httpStatusCode, string jsonString)
        {
            context.Response.StatusCode = httpStatusCode;
            context.Response.ContentType = TypeIdentifier.JSONHttpContentMediaType;
            context.Response.ContentLength = jsonString != null ? Encoding.UTF8.GetByteCount(jsonString) : 0;
            await context.Response.WriteAsync(jsonString);
        }

        private string ConvertToJSONString(int httpStatusCode, object content, string httpMethod)
            => JsonConvert.SerializeObject(new ApiResponse($"{httpMethod} {ResponseMessage.Success}", content, !_options.ShowStatusCode ? 0 : httpStatusCode , GetApiVersion()), _jsonSettings);

        private string ConvertToJSONString(ApiResponse apiResponse)
        {
            apiResponse.StatusCode = !_options.ShowStatusCode ? 0 : apiResponse.StatusCode;
            return JsonConvert.SerializeObject(apiResponse, _jsonSettings);
        }

        private string ConvertToJSONString(ApiError apiError) => JsonConvert.SerializeObject(apiError, _jsonSettings);

        private string ConvertToJSONString(object rawJSON) => JsonConvert.SerializeObject(rawJSON, _jsonSettings);

        private ApiError WrapUnsucessfulError(int statusCode) =>
            statusCode switch
            {
                Status204NoContent => new ApiError(ResponseMessage.NotContent),
                Status400BadRequest => new ApiError(ResponseMessage.BadRequest),
                Status401Unauthorized => new ApiError(ResponseMessage.UnAuthorized),
                Status404NotFound =>  new ApiError(ResponseMessage.NotFound),
                Status405MethodNotAllowed => new ApiError(ResponseMessage.MethodNotAllowed),
                Status415UnsupportedMediaType => new ApiError(ResponseMessage.MediaTypeNotSupported),
                _ => new ApiError(ResponseMessage.Unknown)

            };


        private ApiResponse GetErrorResponse(int httpStatusCode, object apiError) 
            => new ApiResponse(!_options.ShowStatusCode ? 0 : httpStatusCode, apiError) { Version = GetApiVersion() };

        private ApiResponse GetSucessResponse(ApiResponse apiResponse, string httpMethod)
        {
            apiResponse.Message ??= $"{httpMethod} {ResponseMessage.Success}";
            apiResponse.Version = GetApiVersion();
            return apiResponse;
        }

        private string GetApiVersion() => !_options.ShowApiVersion ? null : _options.ApiVersion;

        private (bool, object) ValidateSingleValueType(object value)
        {
            var result = value.ToString();
            if (result.IsWholeNumber()) { return (true, result.ToInt64()); }
            if (result.IsDecimalNumber()) { return (true, result.ToDecimal()); }
            if (result.IsBoolean()) { return (true, result.ToBoolean()); }

            return (false, value);
        }

        #endregion
    }

}
