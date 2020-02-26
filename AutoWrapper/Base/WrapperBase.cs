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
        public WrapperBase(RequestDelegate next, AutoWrapperOptions options, ILogger<AutoWrapperMiddleware> logger, IActionResultExecutor<ObjectResult> executor)
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
                var request = await awm.FormatRequestAsync(context.Request);
                var originalResponseBodyStream = context.Response.Body;

                using var memoryStream = new MemoryStream();

                try
                {
                  
                    context.Response.Body = memoryStream;
                    await _next.Invoke(context);

                    if (context.Response.HasStarted) { Log(); return;  }

                    var bodyAsText = await awm.ReadResponseBodyStreamAsync(memoryStream);
                    context.Response.Body = originalResponseBodyStream;

                    var actionIgnore = context.Response.Headers[TypeIdentifier.AutoWrapIgnoreFilterHeader];
                    if (actionIgnore.Count > 0)
                    {
                        await awm.WrapIgnoreAsync(context, bodyAsText); return;
                    }

                    if (context.Response.StatusCode != Status304NotModified || context.Response.StatusCode != Status204NoContent)
                    {

                        if (!_options.IsApiOnly
                            && (bodyAsText.IsHtml()
                            && !_options.BypassHTMLValidation)
                            && context.Response.StatusCode == Status200OK)
                        { context.Response.StatusCode = Status404NotFound; }

                        if (!context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith))
                            && bodyAsText.IsHtml() 
                            && context.Response.StatusCode == Status200OK)
                        {
                            if (memoryStream.Length > 0) { await awm.HandleNotApiRequestAsync(context); }
                            return;
                        }
                        
                        //if (context.Response.StatusCode == Status200OK 
                        //    || context.Response.StatusCode == Status201Created 
                        //    || context.Response.StatusCode == Status202Accepted)
                        if(awm.IsRequestSuccessful(context.Response.StatusCode))
                        {
                            await awm.HandleSuccessfulRequestAsync(context, bodyAsText, context.Response.StatusCode);
                        }
                        else
                        {
                            if (_options.UseApiProblemDetailsException) { await awm.HandleProblemDetailsExceptionAsync(context, _executor, bodyAsText); return; }
                            
                            await awm.HandleUnsuccessfulRequestAsync(context, bodyAsText, context.Response.StatusCode);
                        }
                    }

                }
                catch (Exception exception)
                {
                    if (context.Response.HasStarted) { Log(); return; }

                    if (_options.UseApiProblemDetailsException) { await awm.HandleProblemDetailsExceptionAsync(context, _executor, null, exception); }
                    else { await awm.HandleExceptionAsync(context, exception); }
                    await awm.RevertResponseBodyStreamAsync(memoryStream, originalResponseBodyStream);
                }
                finally
                {
                    LogResponse(context, request, stopWatch);
                }

            }

        }

        private void LogResponse(HttpContext context, string request, Stopwatch stopWatch)
        {
            stopWatch.Stop();
            if (_options.EnableResponseLogging)
            {
                _logger.Log(LogLevel.Information, $@"Source:[{context.Connection.RemoteIpAddress.ToString() }] 
                                                     Request: {request} 
                                                     Responded with [{context.Response.StatusCode}] in {stopWatch.ElapsedMilliseconds}ms");
            }
        }

        private void Log()
        {
            _logger.Log(LogLevel.Warning, "The response has already started, the AutoWrapper middleware will not be executed.");
        }
    }

}
