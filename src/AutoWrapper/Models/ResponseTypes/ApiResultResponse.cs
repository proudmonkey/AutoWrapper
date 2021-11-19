namespace AutoWrapper.Models.ResponseTypes
{
    public class ApiResultResponse<T> : ApiResultResponse
    {
        public T Result { get; set; } = default;
    }

    public class ApiResultResponse
    {
        public bool IsError { get; set; }
        public int Status { get; set; }
        public string? Message { get; set; } = null!;
    }
}
