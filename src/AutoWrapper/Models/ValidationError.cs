namespace AutoWrapper.Models
{
    public class ValidationError
    {
        public string? Name { get; }
        public string Reason { get; }
        public ValidationError(string name, string reason)
        {
            Name = name != string.Empty ? name : null;
            Reason = reason;
        }
    }
}
