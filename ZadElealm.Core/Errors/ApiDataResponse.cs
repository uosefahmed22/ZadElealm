namespace ZadElealm.Apis.Errors
{
    public class ApiDataResponse : ApiResponse
    {
        public object Data { get; set; }
        public ApiDataResponse(int statusCode, object data = null, string message = null) : base(statusCode, message)
        {
            Data = data;
        }
    }
}