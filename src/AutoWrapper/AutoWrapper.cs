using Microsoft.AspNetCore.Builder;

namespace AutoWrapper
{
    public static class AutoWrapperExtension
    {
        public static IApplicationBuilder UseAutoWrapper(this IApplicationBuilder builder, AutoWrapperOptions? options = null)
        {
            options ??= new AutoWrapperOptions();
            return builder.UseMiddleware<AutoWrapperMiddleware>(options);
        }
    }
}
