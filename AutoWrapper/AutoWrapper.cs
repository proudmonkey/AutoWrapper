using Microsoft.AspNetCore.Builder;
using System;

namespace AutoWrapper
{
    public static class AutoWrapperExtension
    {
        public static IApplicationBuilder UseApiResponseAndExceptionWrapper(this IApplicationBuilder builder, AutoWrapperOptions options = default)
        {
            options ??= new AutoWrapperOptions();
            return builder.UseMiddleware<AutoWrapperMiddleware>(options);
        }
    }
}
