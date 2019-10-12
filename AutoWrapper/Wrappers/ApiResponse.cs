using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace AutoWrapper.Wrappers
{
    //[DataContract]
    public class ApiResponse
    {
        ////[DataMember(EmitDefaultValue = false)]
        //[JsonProperty("version")]
        public string Version { get; set; }

       ////[DataMember(EmitDefaultValue = false)]
       // [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        ////[DataMember(EmitDefaultValue = false)]
        //[JsonProperty("message")]
        public string Message { get; set; }

        ////[DataMember]
        //[JsonProperty("isError")]
        public bool IsError { get; set; }

        ////[DataMember(EmitDefaultValue = false)]
        //[JsonProperty("responseException")]
        public ApiError ResponseException { get; set; }

        ////[DataMember(EmitDefaultValue = false)]
        //[JsonProperty("result")]
        public object Result { get; set; }

        [JsonConstructor]
        public ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Result = result;
            this.Version = apiVersion;
            this.IsError = false;
        }

        public ApiResponse(int statusCode , ApiError apiError)
        {
            this.StatusCode = statusCode;
            this.ResponseException = apiError;
            this.IsError = true;
        }
    }
}

