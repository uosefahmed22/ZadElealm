using System.Collections.Concurrent;
using System.Text.Json;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Middlwares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, ClientStatistics> _clientStatistics =
            new ConcurrentDictionary<string, ClientStatistics>();
        private readonly int _maxRequests;
        private readonly TimeSpan _interval;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly Timer _cleanupTimer;

        public RateLimitingMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            _maxRequests = configuration.GetValue<int>("RateLimiting:MaxRequests", 100);
            _interval = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:IntervalSeconds", 60));

            _cleanupTimer = new Timer(CleanupOldEntries, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                          context.Connection.RemoteIpAddress?.ToString() ??
                          "unknown";

            var path = context.Request.Path.Value ?? "";
            var maxRequestsForEndpoint = GetMaxRequestsForEndpoint(path);

            if (IsRateLimitExceeded(clientId, maxRequestsForEndpoint))
            {
                _logger.LogWarning("Rate limit exceeded: {ClientIP} attempted too many requests to {Path}", clientId, path);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Add("Retry-After", "60");
                context.Response.ContentType = "application/json";

                var response = new ApiResponse(
                    StatusCodes.Status429TooManyRequests,
                    "Too many requests. Please try again later."
                );

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
                return;
            }

            await _next(context);
        }

        private int GetMaxRequestsForEndpoint(string path)
        {
            if (path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
                return _maxRequests / 2;

            if (path.StartsWith("/api/public", StringComparison.OrdinalIgnoreCase))
                return _maxRequests * 2; 

            if (path.EndsWith(".js") || path.EndsWith(".css") || path.EndsWith(".png") || path.EndsWith(".jpg"))
                return _maxRequests * 5;

            return _maxRequests; 
        }
        private bool IsRateLimitExceeded(string clientId, int maxRequests)
        {
            var clientStats = _clientStatistics.AddOrUpdate(
                clientId,
                _ => new ClientStatistics { LastRequestTime = DateTime.UtcNow, RequestCount = 1 },
                (_, stats) =>
                {
                    if (DateTime.UtcNow - stats.LastRequestTime > _interval)
                    {
                        stats.RequestCount = 1;
                        stats.LastRequestTime = DateTime.UtcNow;
                    }
                    else
                    {
                        stats.RequestCount++;
                    }
                    return stats;
                });

            return clientStats.RequestCount > maxRequests;
        }
        private void CleanupOldEntries(object state)
        {
            try
            {
                int removedEntries = 0;

                foreach (var key in _clientStatistics.Keys)
                {
                    if (_clientStatistics.TryGetValue(key, out var stats))
                    {
                        if (DateTime.UtcNow - stats.LastRequestTime > _interval.Add(_interval))
                        {
                            if (_clientStatistics.TryRemove(key, out _))
                            {
                                removedEntries++;
                            }
                        }
                    }
                }

                if (removedEntries > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired rate limiting entries", removedEntries);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rate limiting cleanup");
            }
        }
    }
}