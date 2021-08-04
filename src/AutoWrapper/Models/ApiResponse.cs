namespace AutoWrapper.Models
{
    using AutoWrapper.Interface;
    using System.Text.Json.Serialization;

    public class ApiResponse : IApiResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsError { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Result { get; set; }

        public ApiResponse(string message, object? result = null, int? statusCode = null)
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
        }
        public ApiResponse(object result, int statusCode = 200)
        {
            StatusCode = statusCode == 0 ? null : statusCode;
            Result = result;
        }

        public ApiResponse() { }
    }
}


