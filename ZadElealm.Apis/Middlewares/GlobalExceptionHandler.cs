using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ZadElealm.Apis.Middlwares
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "حدث خطأ غير متوقع.",
                Detail = "يرجى المحاولة لاحقاً.",
                Instance = httpContext.Request.Path

            };
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
            return true;
        }
    }
}
