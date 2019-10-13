using System.ComponentModel;

namespace AutoWrapper.Wrappers
{
    internal enum ResponseMessageEnum
    {
        [Description("Request successful.")]
        Success,
        [Description("Request not found. The specified uri does not exist.")]
        NotFound,
        [Description("Request responded with 'Method Not Allowed'.")]
        MethodNotAllowed,
        [Description("Request no content. The specified uri does not contain any content.")]
        NotContent,
        [Description("Request responded with exceptions.")]
        Exception,
        [Description("Request denied. Unauthorized access.")]
        UnAuthorized,
        [Description("Request responded with validation error(s). Please correct the specified validation errors and try again.")]
        ValidationError,
        [Description("Request cannot be processed. Please contact a support.")]
        Unknown,
        [Description("Unhandled Exception occured. Unable to process the request.")]
        Unhandled,
        [Description(@"AutoWrapper is meant to be used for ASP.NET Core API project templates only. 
                      If you are combining API Controllers within your front-end projects like Angular, MVC, React, Blazor and other SPA frameworks that supports .NET Core,
                      then set the AutoWrapperOptions IsApiOnly property to false.")]
        NotApiOnly
    }

   
}
