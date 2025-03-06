namespace ZadElealm.Apis.Errors
{
    public class ApiDataResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public ApiDataResponse(int statusCode, T data, string message = null)
            : base(statusCode, message)
        {
            Data = data;
        }

        public ApiDataResponse(int statusCode, string message = null)
            : base(statusCode, message)
        {
            Data = default;
        }
    }
}
