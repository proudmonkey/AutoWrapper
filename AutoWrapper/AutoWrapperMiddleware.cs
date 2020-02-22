using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AutoWrapper.Base;

namespace AutoWrapper
{
    internal class AutoWrapperMiddleware : WrapperBase
    {
        private readonly AutoWrapperMembers _awm;
        public AutoWrapperMiddleware(RequestDelegate next, AutoWrapperOptions options, ILogger<AutoWrapperMiddleware> logger) : base(next, options, logger)
        {
            var jsonSettings = Helpers.JSONHelper.GetJSONSettings(options.IgnoreNullValue, options.ReferenceLoopHandling, options.UseCamelCaseNamingStrategy);
            _awm = new AutoWrapperMembers(options, logger, jsonSettings);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await InvokeAsyncBase(context, _awm);
        }
    }

    internal class AutoWrapperMiddleware<T> : WrapperBase
    {
        private readonly AutoWrapperMembers _awm;
        public AutoWrapperMiddleware(RequestDelegate next, AutoWrapperOptions options, ILogger<AutoWrapperMiddleware> logger) : base(next, options, logger)
        {
            var (Settings, Mappings) = Helpers.JSONHelper.GetJSONSettings<T>(options.IgnoreNullValue, options.ReferenceLoopHandling, options.UseCamelCaseNamingStrategy);
            _awm = new AutoWrapperMembers(options, logger, Settings, Mappings, true);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await InvokeAsyncBase(context, _awm);
        }

    }
}
