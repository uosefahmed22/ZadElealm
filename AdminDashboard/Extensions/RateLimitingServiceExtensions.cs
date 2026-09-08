using AdminDashboard.Middlwares;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace AdminDashboard.Extentions;

public static class RateLimitingServiceExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("admin-login", limiter =>
            {
                limiter.PermitLimit = configuration.GetValue("RateLimiting:LoginPermitLimit", 5);
                limiter.Window = TimeSpan.FromMinutes(configuration.GetValue("RateLimiting:LoginWindowMinutes", 1));
                limiter.QueueLimit = 0;
                limiter.AutoReplenishment = true;
            });
        });

        return services;
    }
}
