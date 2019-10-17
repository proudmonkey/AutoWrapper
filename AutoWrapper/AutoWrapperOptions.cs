using AutoWrapper.Base;

namespace AutoWrapper
{
    public class AutoWrapperOptions :OptionBase
    {
        public bool UseCustomSchema { get; set; } = false;
    }

    public class AutoWrapperOptions<T> :OptionBase
    {    
    }
}
