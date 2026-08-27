using AdminDashboard.Middlwares;

namespace AdminDashboard.Extentions
{
    public static class RateLimitingServiceExtensions
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));
            return services;
        }
    }
}
