
namespace AutoWrapper
{
    public class AutoWrapperOptions :OptionBase
    {
        ///// <summary>
        ///// Sets the Api version to be shown in the response. You must set the ShowApiVersion to true to see this value in the response.
        ///// </summary>
        //public string ApiVersion { get; set; } = "1.0.0.0";
        ///// <summary>
        ///// Shows the stack trace information in the responseException details.
        ///// </summary>
        //public bool IsDebug { get; set; } = false;
        ///// <summary>
        ///// Shows the Api Version attribute in the response.
        ///// </summary>
        //public bool ShowApiVersion { get; set; } = false;

        ///// <summary>
        ///// Shows the StatusCode attribute in the response.
        ///// </summary>
        //public bool ShowStatusCode { get; set; } = false;

        ///// <summary>
        ///// Use to indicate if the wrapper is used for API project only. Set this to false when you want to use the wrapper within an Angular, MVC, React or Blazor projects.
        ///// </summary>
        //public bool IsApiOnly { get; set; } = true;

        ///// <summary>
        ///// Set the Api path segment to validate. The default value is '/api'. Only works if IsApiOnly is set to false.
        ///// </summary>
        //public string WrapWhenApiPathStartsWith { get; set; } = "/api";

        //public bool IgnoreNullValue { get; set; } = true;

       
    }

    public class AutoWrapperOptions<T> :OptionBase
    {
        
    }
}
