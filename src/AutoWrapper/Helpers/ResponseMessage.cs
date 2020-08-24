namespace AutoWrapper.Helpers
{
    internal class ResponseMessage
    {
        internal const string Success = "Request successful.";
        internal const string NotFound = "Request not found. The specified uri does not exist.";
        internal const string BadRequest = "Request invalid.";
        internal const string MethodNotAllowed = "Request responded with 'Method Not Allowed'.";
        internal const string NotContent = "Request no content. The specified uri does not contain any content.";
        internal const string Exception = "Request responded with exceptions.";
        internal const string UnAuthorized = "Request denied. Unauthorized access.";
        internal const string ValidationError = "Request responded with one or more validation errors.";
        internal const string Unknown = "Request cannot be processed. Please contact support.";
        internal const string Unhandled = "Unhandled Exception occurred. Unable to process the request.";
        internal const string MediaTypeNotSupported = "Unsupported Media Type.";
        internal const string NotApiOnly = @"HTML detected in the response body.
If you are combining API Controllers within your front-end projects like Angular, MVC, React, Blazor and other SPA frameworks that supports .NET Core, then set the AutoWrapperOptions IsApiOnly property to false. 
If you are using pure API and want to output HTML as part of your JSON object, then set BypassHTMLValidation property to true.";
        internal const string NoMappingFound = "You must apply the [AutoWrapperPropertyMap] Attribute to map through the default ApiResponse properties. If you want to to define your own custom response,  set UseCustomSchema = true in the AutoWrapper options.";
    }
}
