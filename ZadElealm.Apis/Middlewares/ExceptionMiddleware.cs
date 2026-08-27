using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using System.Net;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Middlwares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse(context.Response.StatusCode, ex.Message);

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
