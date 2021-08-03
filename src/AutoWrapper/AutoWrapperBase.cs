using AutoWrapper.Attributes;
using AutoWrapper.Handlers;
using HelpMate.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace AutoWrapper.Base
{
    internal abstract class AutoWrapperBase
    {
        private readonly RequestDelegate _next;
        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        private bool _isRequestOk = false;

        private IActionResultExecutor<ObjectResult> Executor { get; }

        public AutoWrapperBase(RequestDelegate next, 
                          AutoWrapperOptions options, 
                          ILogger<AutoWrapperMiddleware> logger, 
                          IActionResultExecutor<ObjectResult> executor)
        {
            _next = next;
            _options = options;
            _logger = logger;
            Executor = executor;
        }

        public virtual async Task InvokeAsyncBase(HttpContext context, ApiRequestHandler requestHandler)
        {
            if (requestHandler.ShouldIgnoreRequest(context, _options.ExcludePaths))
            {
                await _next(context);
            }
            else
            {
                await InvokeNextAsync(context, requestHandler);
            }
        }

        private async Task InvokeNextAsync(HttpContext context, ApiRequestHandler requestHandler)
        {
            if (context.Response.HasStarted)
            {
                LogResponseHasStartedError();
                return;
            }

            var stopWatch = Stopwatch.StartNew();
            var requestBody = await requestHandler.GetRequestBodyAsync(context.Request);
            var originalResponseBodyStream = context.Response.Body;


            using var memoryStream = new MemoryStream();

            try
            {
                context.Response.Body = memoryStream;
                await _next.Invoke(context);

                var endpoint = context.GetEndpoint();

                if (endpoint?.Metadata?.GetMetadata<AutoWrapIgnoreAttribute>() is object)
                {
                    await requestHandler.RevertResponseBodyStreamAsync(memoryStream, originalResponseBodyStream);
                    return;
                }

                if (context.Response.StatusCode != Status304NotModified && context.Response.StatusCode != Status204NoContent)
                {
                    await HandleRequestAsync(context, requestHandler, memoryStream, originalResponseBodyStream);
                }
            }
            catch (Exception exception)
            {

                if (_options.UseApiProblemDetailsException)
                {
                    await requestHandler.HandleProblemDetailsExceptionAsync(context, Executor, null, exception);
                    return;
                }

                await requestHandler.HandleExceptionAsync(context, exception);
                await requestHandler.RevertResponseBodyStreamAsync(memoryStream, originalResponseBodyStream);
            }
            finally
            {
                LogHttpRequest(context, requestBody, stopWatch, _isRequestOk);
            }
        }

        private async Task HandleRequestAsync(HttpContext context, ApiRequestHandler requestHandler, MemoryStream memoryStream, Stream bodyStream)
        {
            var (_, ParsedText, JsonDoc) = await requestHandler.ReadResponseBodyStreamAsync(memoryStream);
            var bodyAsText = ParsedText;

            context.Response.Body = bodyStream;

            var isPageRequest = !_options.IsApiOnly
                    && (bodyAsText.IsHtml()
                    && !_options.BypassHTMLValidation)
                    && context.Response.StatusCode == Status200OK;

            if (isPageRequest)
            { 
                context.Response.StatusCode = Status404NotFound; 
            }

            if (isPageRequest && !context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith)))
            {
                if (memoryStream.Length > 0) 
                { 
                    await requestHandler.HandleNotApiRequestAsync(context);
                    return;
                }
            }

            _isRequestOk = ApiRequestHandler.IsRequestSuccessful(context.Response.StatusCode);

            if (_isRequestOk)
            {
                if (_options.IgnoreWrapForOkRequests)
                {
                    await requestHandler.WrapIgnoreAsync(context, bodyAsText);
                    return;
                }

                await requestHandler.HandleSuccessfulRequestAsync(context, bodyAsText, context.Response.StatusCode, JsonDoc);
                return;
            }

            if (_options.UseApiProblemDetailsException)
            {
                await requestHandler.HandleProblemDetailsExceptionAsync(context, Executor, bodyAsText);
                return;
            }

            await requestHandler.HandleUnsuccessfulRequestAsync(context, bodyAsText, context.Response.StatusCode);
        }

        private bool ShouldLogRequestData(HttpContext context)
        {
            if (_options.ShouldLogRequestData)
            {
                var endpoint = context.GetEndpoint();
                return !(endpoint?.Metadata?.GetMetadata<RequestDataLogIgnoreAttribute>() is object);
            }

            return false;
        }

        private void LogHttpRequest(HttpContext context, string? requestBody, Stopwatch stopWatch, bool isRequestOk)
        {
            stopWatch.Stop();
            if (_options.EnableResponseLogging)
            {
                bool shouldLogRequestData = ShouldLogRequestData(context);

                var request = shouldLogRequestData
                            ? isRequestOk
                                ? $"{context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} {requestBody}"
                                : (!isRequestOk && _options.LogRequestDataOnException)
                                   ? $"{context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} {requestBody}"
                                   : $"{context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path}"
                            : $"{context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path}";

                _logger.Log(LogLevel.Information, $"Source:[{context.Connection.RemoteIpAddress }] " +
                                                  $"Request: {request} " +
                                                  $"Responded with [{context.Response.StatusCode}] in {stopWatch.ElapsedMilliseconds}ms");
            }
        }

        private void LogResponseHasStartedError()
        {
            _logger.Log(LogLevel.Warning, "The response has already started, the AutoWrapper middleware will not be executed.");
        }
    }

}
