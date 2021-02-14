namespace AutoWrapper.Models.ResponseTypes
{
    public class ApiResultResponse<T> where T : class
    {
        public string Message { get; set; } = null!;
        public T Result { get; set; } = null!;
    }
}
