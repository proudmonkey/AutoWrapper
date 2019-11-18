using AutoWrapper.Extensions;
using Microsoft.AspNetCore.Http;
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
        public WrapperBase(RequestDelegate next, AutoWrapperOptions options, ILogger<AutoWrapperMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public virtual async Task InvokeAsyncBase(HttpContext context, AutoWrapperMembers awm)
        {
            if (awm.IsSwagger(context) || !awm.IsApi(context))
                await this._next(context);
            else
            {

                var stopWatch = Stopwatch.StartNew();

                var request = await awm.FormatRequest(context.Request);

                var originalBodyStream = context.Response.Body;

                using (var newBodyStream = new MemoryStream())
                {
                    try
                    {
                        context.Response.Body = newBodyStream;
                        await _next.Invoke(context);

                        context.Response.Body = originalBodyStream;

                        var bodyAsText = await awm.FormatResponse(newBodyStream);

                        var actionIgnore = context.Response.Headers["AutoWrapIgnoreFilter"];
                        if (actionIgnore.Count > 0)
                        {
                            await awm.WrapIgnoreAsync(context, bodyAsText);return;
                        }

                        if (context.Response.StatusCode != Status304NotModified)
                        {

                            if (!_options.IsApiOnly && bodyAsText.IsHtml() && context.Response.StatusCode == Status200OK)
                                context.Response.StatusCode = Status404NotFound;

                            if (!context.Request.Path.StartsWithSegments(new PathString(_options.WrapWhenApiPathStartsWith))
                                && bodyAsText.IsHtml() && context.Response.StatusCode == Status200OK)
                            {
                                if (newBodyStream.Length > 0)
                                {
                                    await awm.HandleSpaSupportAsync(context); return;
                                }
                            }
                            else if (context.Response.StatusCode == Status200OK)
                            {
                                await awm.HandleSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            }
                            else
                            {
                                await awm.HandleNotSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        await awm.HandleExceptionAsync(context, ex);
                        newBodyStream.Seek(0, SeekOrigin.Begin);
                        await newBodyStream.CopyToAsync(originalBodyStream);
                    }
                    finally
                    {
                        LogResponse(context, request, stopWatch);
                    }
                }

            }

        }

        private void LogResponse(HttpContext context, string request, Stopwatch stopWatch)
        {
            stopWatch.Stop();
            if (_options.EnableResponseLogging)
                _logger.Log(LogLevel.Information, $@"Source:[{context.Connection.RemoteIpAddress.ToString() }] Request: {request} Responded with [{context.Response.StatusCode}] in {stopWatch.ElapsedMilliseconds}ms");
        }
    }

}
