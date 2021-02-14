namespace AutoWrapper.Exceptions
{
    using AutoWrapper.Models;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    public class ApiException : Exception
    {
        public int StatusCode { get; init; }

        public string? ErrorCode { get; init; }

        public IEnumerable<ValidationError>? ValidationErrors { get; init; }

        public object? CustomErrorModel { get; init; }

        public ApiException(string message,
                            int statusCode = Status400BadRequest,
                            string? errorCode = null) :
            
            base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public ApiException(object? customErrorModel, int statusCode = Status400BadRequest)
        {
            StatusCode = statusCode;
            CustomErrorModel = customErrorModel;
        }

        
        
        public ApiException(IEnumerable<ValidationError> errors, int statusCode = Status400BadRequest)
        {
            ValidationErrors = errors;
            StatusCode = statusCode;
        }

        public ApiException(IEnumerable<ValidationError> errors, 
                            string? message, 
                            string? code, 
                            int statusCode = Status400BadRequest): 
            base(message)
        {
            ValidationErrors = errors;
            ErrorCode = code;
            StatusCode = statusCode;
        }
        public ApiException(Exception ex, int statusCode = Status500InternalServerError) : base(ex.Message)
        {
            StatusCode = statusCode;
        }
    }
}
