namespace AutoWrapper.Models
{
    public class ApiResultResponse<T> where T : class
    {
        public string Message { get; set; }
        public T Result { get; set; }
    }
}
