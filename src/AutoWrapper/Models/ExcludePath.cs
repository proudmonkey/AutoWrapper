namespace AutoWrapper.Models
{
    public class ExcludePath
    {
        public string? Path { get; set; }

        public ExcludeMode ExcludeMode { get; set; }

        public ExcludePath(string path, ExcludeMode excludeMode = ExcludeMode.Strict)
        {
            Path = path;
            ExcludeMode = excludeMode;
        }
    }

    public enum ExcludeMode
    {
        Strict = 1,
        StartsWith = 2,
        Regex = 3
    }
}
