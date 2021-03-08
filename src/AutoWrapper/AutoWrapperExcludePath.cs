using System;
using System.Collections.Generic;
using System.Text;

namespace AutoWrapper
{
    public enum ExcludeMode
    {
        Strict = 1,
        StartWith = 2,
        Regex = 3
    }

    public class AutoWrapperExcludePath
    {
        public AutoWrapperExcludePath(string path, ExcludeMode excludeMode = ExcludeMode.Strict)
        {
            Path = path;
            ExcludeMode = excludeMode;
        }

        public string Path { get; set; }

        public ExcludeMode ExcludeMode { get; set; }
    }
}
