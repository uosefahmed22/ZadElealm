using System.Collections.Concurrent;
using System.Text.Json;

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

            // Get configuration values with defaults
            _maxRequests = configuration.GetValue<int>("RateLimiting:MaxRequests", 100);
            _interval = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:IntervalSeconds", 60));

            // Set up periodic cleanup timer (runs every 5 minutes)
            _cleanupTimer = new Timer(CleanupOldEntries, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Consider X-Forwarded-For header for clients behind proxies
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
                    "Too many requests. Please try again later after 60 seconds."
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
            // Different rate limits for different types of endpoints
            if (path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
                return _maxRequests / 2; // More strict for admin endpoints

            if (path.StartsWith("/api/public", StringComparison.OrdinalIgnoreCase))
                return _maxRequests * 2; // More lenient for public endpoints

            if (path.EndsWith(".js") || path.EndsWith(".css") || path.EndsWith(".png") || path.EndsWith(".jpg"))
                return _maxRequests * 5; // Static resources need higher limits

            return _maxRequests; // Default limit
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
                        // Reset if interval has passed
                        stats.RequestCount = 1;
                        stats.LastRequestTime = DateTime.UtcNow;
                    }
                    else
                    {
                        // Increment request count
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

                // Remove entries older than twice the interval
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

    public class ClientStatistics
    {
        public DateTime LastRequestTime { get; set; }
        public int RequestCount { get; set; }
    }

    // Assuming you have an ApiResponse class like this:
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public ApiResponse(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
