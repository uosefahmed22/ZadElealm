using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Middlwares
{
    public class ApiExceptionMiddleware : ApiResponse
    {
        public string? Details { get; set; }
        public ApiExceptionMiddleware(int statusCode, string? message = null, string? details = null) : base(statusCode, message)
        {
            Details = details;
        }
    }
}
