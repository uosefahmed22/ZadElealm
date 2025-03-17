namespace ZadElealm.Apis.Errors
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public ApiResponse(int statusCode)
        {
            StatusCode = statusCode;
            Message = GetDefaultMessageForStatusCode(statusCode);
        }

        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }
        protected string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Resource not found",
                500 => "An error occurred. Try again",
                _ => null
            };
        }
    }
}
