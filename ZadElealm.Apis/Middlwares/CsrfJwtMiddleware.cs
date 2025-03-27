using Microsoft.AspNetCore.Antiforgery;
using System.IdentityModel.Tokens.Jwt;

namespace ZadElealm.Apis.Middlwares
{
    public class CsrfJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public CsrfJwtMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method != "GET") 
            {
                var csrfToken = context.Request.Headers["X-CSRF-TOKEN"].ToString();
                if (string.IsNullOrEmpty(csrfToken))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsJsonAsync(new { message = "CSRF token is missing" });
                    return;
                }

                var jwtToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "JWT token is missing" });
                    return;
                }

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwt = tokenHandler.ReadJwtToken(jwtToken);
                    var csrfClaim = jwt.Claims.FirstOrDefault(c => c.Type == "csrf-token");

                    if (csrfClaim == null || csrfClaim.Value != csrfToken)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { message = "Invalid CSRF token" });
                        return;
                    }
                }
                catch
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsJsonAsync(new { message = "Invalid token" });
                    return;
                }
            }

            await _next(context);
        }
    }
}
