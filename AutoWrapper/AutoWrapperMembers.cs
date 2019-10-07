using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace AutoWrapper
{
    internal class AutoWrapperMembers
    {

        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        public AutoWrapperMembers(AutoWrapperOptions options, ILogger<AutoWrapperMiddleware> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return $"{request.Method} {request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        public async Task<string> FormatResponse(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(bodyStream).ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);

            return plainBodyText;
        }

        public Task HandleExceptionAsync(HttpContext context, System.Exception exception)
        {
            ApiError apiError = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                if (ex.IsModelValidatonError)
                {
                    apiError = new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ex.Errors)
                    {
                        ReferenceErrorCode = ex.ReferenceErrorCode,
                        ReferenceDocumentLink = ex.ReferenceDocumentLink,
                    };

                    _logger.Log(LogLevel.Warning, exception, $"[{ex.StatusCode}]: {ResponseMessageEnum.ValidationError.GetDescription()}");
                }
                else
                {
                    apiError = new ApiError(ex.Message)
                    {
                        ReferenceErrorCode = ex.ReferenceErrorCode,
                        ReferenceDocumentLink = ex.ReferenceDocumentLink,
                    };

                    _logger.Log(LogLevel.Warning, exception, $"[{ex.StatusCode}]: {ResponseMessageEnum.Exception.GetDescription()}");
                }

                code = ex.StatusCode;
                //context.Response.StatusCode = code;

            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
                code = (int)HttpStatusCode.Unauthorized;

                _logger.Log(LogLevel.Warning, exception, $"[{code}]: {ResponseMessageEnum.UnAuthorized.GetDescription()}");
            }
            else
            {

                string exceptionMessage = string.Empty;
                string stackTrace = null;

                if (_options.IsDebug)
                {
                    exceptionMessage = $"{ exceptionMessage } { exception.GetBaseException().Message }";
                    stackTrace = exception.StackTrace;
                }
                else
                {
                    exceptionMessage = ResponseMessageEnum.Unhandled.GetDescription();
                }

                apiError = new ApiError(exceptionMessage) { Details = stackTrace };
                code = (int)HttpStatusCode.InternalServerError;

                _logger.Log(LogLevel.Error, exception, $"[{code}]: {exceptionMessage}");
            }

            var jsonString = ConvertToJSONString(GetErrorResponse(code, apiError));

            return WriteFormattedResponseToHttpContext(context, code, jsonString);
        }

        public Task HandleNotSuccessRequestAsync(HttpContext context, int code)
        {
            ApiError apiError = WrapError(code);
            var jsonString = ConvertToJSONString(GetErrorResponse(code, apiError));
            return WriteFormattedResponseToHttpContext(context, code, jsonString);
        }

        public Task HandleSuccessRequestAsync(HttpContext context, object body, int code)
        {
            string jsonString = string.Empty;

            var bodyText = !body.ToString().IsValidJson() ? ConvertToJSONString(body) : body.ToString();

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type = bodyContent?.GetType();

            if (type.Equals(typeof(JObject)))
            {
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);
                if (apiResponse.StatusCode == 0 && apiResponse.Result == null && apiResponse.ResponseException == null)
                    jsonString = ConvertToJSONString(code, bodyContent);
                else if ((apiResponse.StatusCode != code || apiResponse.Result != null) ||
                        (apiResponse.StatusCode == code && apiResponse.Result == null))
                {
                    code = apiResponse.StatusCode; // in case response is not 200 (e.g 201, etc)
                    jsonString = ConvertToJSONString(GetSucessResponse(apiResponse));
                }
                else
                    jsonString = ConvertToJSONString(code, bodyContent);
            }
            else
            {
                jsonString = ConvertToJSONString(code, bodyContent);
            }

            return WriteFormattedResponseToHttpContext(context, code, jsonString);
        }

        public Task HandleSpaSupportAsync(HttpContext context)
        {
            string configErrorText = ResponseMessageEnum.NotApiOnly.GetDescription();
            context.Response.ContentLength = configErrorText != null ? System.Text.Encoding.UTF8.GetByteCount(configErrorText) : 0;
            return context.Response.WriteAsync(configErrorText);
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

        #region Private Members

        private Task WriteFormattedResponseToHttpContext(HttpContext context, int code, string jsonString)
        {
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = jsonString != null ? System.Text.Encoding.UTF8.GetByteCount(jsonString) : 0;
            return context.Response.WriteAsync(jsonString);
        }

        private string ConvertToJSONString(int code, object content)
        {
            code = !_options.ShowStatusCode ? 0 : code;
            return JsonConvert.SerializeObject(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), content, code, GetApiVersion()), JSONSettings());
        }

        private string ConvertToJSONString(ApiResponse apiResponse)
        {
            apiResponse.StatusCode = !_options.ShowStatusCode ? 0 : apiResponse.StatusCode;
            return JsonConvert.SerializeObject(apiResponse, JSONSettings());
        }

        private string ConvertToJSONString(object rawJSON)
        {
            return JsonConvert.SerializeObject(rawJSON, JSONSettings());
        }

        private JsonSerializerSettings JSONSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }

        private ApiError WrapError(int statusCode)
        {
            switch (statusCode)
            {
                case (int)HttpStatusCode.NotFound:
                    return new ApiError(ResponseMessageEnum.NotFound.GetDescription());
                case (int)HttpStatusCode.NoContent:
                    return new ApiError(ResponseMessageEnum.NotContent.GetDescription());
                case (int)HttpStatusCode.MethodNotAllowed:
                    return new ApiError(ResponseMessageEnum.MethodNotAllowed.GetDescription());
                default:
                    return new ApiError(ResponseMessageEnum.Unknown.GetDescription());
            }
        }

        private ApiResponse GetErrorResponse(int code, ApiError apiError)
        {
            code = !_options.ShowStatusCode ? 0 : code;
            return new ApiResponse(code, apiError) { Version = !_options.ShowApiVersion ? null : GetApiVersion() };
        }

        private ApiResponse GetSucessResponse(ApiResponse apiResponse)
        {
            if (_options.ShowApiVersion)
            {
                if (apiResponse.Version.Equals("1.0.0.0"))
                    apiResponse.Version = GetApiVersion();
            }
            else
                apiResponse.Version = null;

            return apiResponse;
        }

        private string GetApiVersion()
        {
            if (_options.ShowApiVersion)
                return string.IsNullOrEmpty(_options.ApiVersion) ? "1.0.0.0" : _options.ApiVersion;

            return null;
        }

        #endregion
    }
}
