using System.Collections.Concurrent;
using System.Text.Json;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Middlwares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, ClientStatistics> _clientStatistics =
            new ConcurrentDictionary<string, ClientStatistics>();

        private readonly int _maxRequests; 
        private readonly TimeSpan _interval; 

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
            _maxRequests = 100; 
            _interval = TimeSpan.FromMinutes(1); 
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (IsRateLimitExceeded(clientId))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse(
                    StatusCodes.Status429TooManyRequests,
                    "Too many requests, please try again later, please wait a few seconds and try again."
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
        private bool IsRateLimitExceeded(string clientId)
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

            // Clean up old entries periodically (optional)
            CleanupOldEntries();

            return clientStats.RequestCount > _maxRequests;
        }
        private void CleanupOldEntries()
        {
            // Periodically remove old entries (optional)
            foreach (var key in _clientStatistics.Keys)
            {
                if (_clientStatistics.TryGetValue(key, out var stats))
                {
                    if (DateTime.UtcNow - stats.LastRequestTime > _interval.Add(_interval))
                    {
                        _clientStatistics.TryRemove(key, out _);
                    }
                }
            }
        }
    }

    public class ClientStatistics
    {
        public DateTime LastRequestTime { get; set; }
        public int RequestCount { get; set; }
    }
}