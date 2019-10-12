using System;
using System.Collections.Generic;
using System.Text;

namespace AutoWrapper.Contracts
{
    internal interface IApiResponse
    {
        public string Version { get; set; }

        public int StatusCode { get; set; }

        public bool IsError { get; set; }

        public string Message { get; set; }

        public object ResponseException { get; set; }

        public object Result { get; set; }
    }
}
