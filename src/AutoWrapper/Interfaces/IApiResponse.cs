namespace AutoWrapper.Interface
{
    public interface IApiResponse
    {
        string? Version { get; set; }

        int? StatusCode { get; set; }

        string? Message { get; set; }

        object? Result { get; set; }
    }
}
