using Microsoft.AspNetCore.Antiforgery;

namespace ZadElealm.Apis.Middlwares
{
    public class CsrfMiddleware
    {
        //private readonly RequestDelegate _next;
        //private readonly IAntiforgery _antiforgery;

        //public CsrfMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        //{
        //    _next = next;
        //    _antiforgery = antiforgery;
        //}

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    if (context.Request.Method == "GET")
        //    {
        //        var tokens = _antiforgery.GetAndStoreTokens(context);
        //        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!);
        //    }

        //    await _next(context);
        //}
    }
}
