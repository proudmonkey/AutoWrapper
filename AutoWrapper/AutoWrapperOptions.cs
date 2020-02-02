using AutoWrapper.Base;
using Newtonsoft.Json;

namespace AutoWrapper
{
    public class AutoWrapperOptions :OptionBase
    {
        public bool UseCustomSchema { get; set; } = false;
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; } = ReferenceLoopHandling.Ignore;
        public bool UseStreamReadWhenFormattingRequest { get; set; } = true;
    }

    public class AutoWrapperOptions<T> :OptionBase
    {    
    }
}
