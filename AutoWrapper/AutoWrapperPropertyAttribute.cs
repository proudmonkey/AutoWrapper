using System;

namespace AutoWrapper
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class AutoWrapperPropertyMapAttribute : Attribute
    {
        public string PropertyName { get; set; } = string.Empty;
        public AutoWrapperPropertyMapAttribute(){}
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
      
        public AutoWrapperPropertyMapAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public class Prop
    {
        public const string Version = "Version";
        public const string StatusCode = "StatusCode";
        public const string Message = "Message";
        public const string IsError = "IsError";
        public const string Result = "Result";
        public const string ResponseException = "ResponseException";
        public const string ResponseException_ExceptionMessage = "ExceptionMessage";
        public const string ResponseException_Details = "Details";
        public const string ResponseException_ReferenceErrorCode = "ReferenceErrorCode";
        public const string ResponseException_ReferenceDocumentLink = "ReferenceDocumentLink";
        public const string ResponseException_ValidationErrors = "ValidationErrors";
        public const string ResponseException_ValidationErrors_Field = "Field";
        public const string ResponseException_ValidationErrors_Message = "Message";
    }
}
