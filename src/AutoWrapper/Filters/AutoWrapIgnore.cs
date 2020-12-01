using System;

namespace AutoWrapper.Filters
{
    public class AutoWrapIgnoreAttribute : Attribute
    {
        public bool ShouldLogRequestData{ get; set; } = true;

        public AutoWrapIgnoreAttribute(){}
    }
}
