namespace AutoWrapper
{
    using AutoWrapper.Base;
    using AutoWrapper.Handlers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    internal class AutoWrapperMiddleware : WrapperBase
    {
        private readonly ApiRequestHandler _handler;
        public AutoWrapperMiddleware(RequestDelegate next, 
                                     AutoWrapperOptions options, 
                                     ILogger<AutoWrapperMiddleware> logger, 
                                     IActionResultExecutor<ObjectResult> executor) 
        : base(next, options, logger, executor)
        {
            var jsonOptions = Configurations.JsonSettings.GetJsonSerializerOptions(options.JsonNamingPolicy, options.IgnoreNullValue);
            _handler = new ApiRequestHandler(options, logger, jsonOptions);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await InvokeAsyncBase(context, _handler);
        }
    }
}
