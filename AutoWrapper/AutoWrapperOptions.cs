using AutoWrapper.Base;
using Newtonsoft.Json;

namespace AutoWrapper
{
    public class AutoWrapperOptions :OptionBase
    {
        public bool UseCustomSchema { get; set; } = false;
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; } = ReferenceLoopHandling.Ignore;
    }

    public class AutoWrapperOptions<T> :OptionBase
    {    
    }
}
