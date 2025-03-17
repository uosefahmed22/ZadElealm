namespace ZadElealm.Apis.Errors
{
    public class ApiResponseListOfErrors : ApiResponse
    {
        public List<string> Errors { get; set; }
        public ApiResponseListOfErrors(int statusCode, string message = null, List<string> errors = null) : base(statusCode, message)
        {
            Errors = errors ?? new List<string>();
        }
    }
}
