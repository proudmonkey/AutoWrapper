using AutoWrapper.Extensions;
using AutoWrapper.Helpers;
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
    internal abstract class WrapperBase
    {
        private readonly RequestDelegate _next;
        private readonly AutoWrapperOptions _options;
        private readonly ILogger<AutoWrapperMiddleware> _logger;
        private IActionResultExecutor<ObjectResult> _executor { get; }
        public WrapperBase(RequestDelegate next, 
                          AutoWrapperOptions options, 
                          ILogger<AutoWrapperMiddleware> logger, 
                          IActionResultExecutor<ObjectResult> executor)
        {
            _next = next;
            _options = options;
            _logger = logger;
            _executor = executor;
        }

        public virtual async Task InvokeAsyncBase(HttpContext context, AutoWrapperMembers awm)
        {
            if (awm.IsSwagger(context) || !awm.IsApi(context))
                await _next(context);
            else
            {
                var stopWatch = Stopwatch.StartNew();
                var requestBody = await awm.GetRequestBodyAsync(context.Request);
                var originalResponseBodyStream = context.Response.Body;
                bool isRequestOk = false;

                using var memoryStream = new MemoryStream();

                try
                {
                    context.Response.Body = memoryStream;
                    await _next.Invoke(context);

                    if (context.Response.HasStarted) { LogResponseHasStartedError(); return; }

                    var actionIgnore = context.Request.Headers[TypeIdentifier.AutoWrapIgnoreFilterHeader];
                    if (actionIgnore.Count > 0) 
                    { 
                        await awm.RevertResponseBodyStreamAsync(memoryStream, originalResponseBodyStream);
                        return;
                    }

                    var bodyAsText = await awm.ReadResponseBodyStreamAsync(memoryStream);
                    context.Response.Body = originalResponseBodyStream;

                    if (context.Response.StatusCode != Status304NotModified && context.Response.StatusCode != Status204NoContent)
                    {

                        if (!_options.IsApiOnly
                            && (bodyAsText.IsHtml()
                            && !_options.BypassHTMLValidation)
                            && context.Response.StatusCode == Status200OK)
                        { context.Response.StatusCode = Status404NotFound; }

                        if (!context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith))
                            && (bodyAsText.IsHtml()
                            && !_options.BypassHTMLValidation)
                            && context.Response.StatusCode == Status200OK)
                        {
                            if (memoryStream.Length > 0) { await awm.HandleNotApiRequestAsync(context); }
                            return;
                        }

                        isRequestOk = awm.IsRequestSuccessful(context.Response.StatusCode);
                        if (isRequestOk)
                        {
                            if (_options.IgnoreWrapForOkRequests)
                            {
                                await awm.WrapIgnoreAsync(context, bodyAsText);
                            }
                            else
                            {
                                await awm.HandleSuccessfulRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            }
                        }
                        else
                        {
                            if (_options.UseApiProblemDetailsException) 
                            { 
                                await awm.HandleProblemDetailsExceptionAsync(context, _executor, bodyAsText); 
                                return; 
                            }

                            await awm.HandleUnsuccessfulRequestAsync(context, bodyAsText, context.Response.StatusCode);
                        }
                    }

                }
                catch (Exception exception)
                {
                    if (context.Response.HasStarted) { LogResponseHasStartedError(); return; }

                    if (_options.UseApiProblemDetailsException) 
                    { 
                        await awm.HandleProblemDetailsExceptionAsync(context, _executor, null, exception); 
                    }
                    else 
                    { 
                        await awm.HandleExceptionAsync(context, exception); 
                    }

                    await awm.RevertResponseBodyStreamAsync(memoryStream, originalResponseBodyStream);
                }
                finally
                {
                    LogHttpRequest(context, requestBody, stopWatch, isRequestOk);
                }
            }
        }

        private bool ShouldLogRequestData(HttpContext context)
        {
            if (_options.ShouldLogRequestData)
            {
                return context.Request.Headers[TypeIdentifier.ShouldLogRequestDataFilterHeader].Count > 0
                            ? context.Response.Headers[TypeIdentifier.ShouldLogRequestDataFilterHeader].ToString().ToBoolean()
                            : true;
            }

            return false;
        }

        private void LogHttpRequest(HttpContext context, string requestBody, Stopwatch stopWatch, bool isRequestOk)
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
