using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Helpers;

namespace ZadElealm.Apis.Middlwares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<RateLimitOptions> _options;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientStatistics> _clientStatistics =
            new ConcurrentDictionary<string, ClientStatistics>();

        public RateLimitingMiddleware(
            RequestDelegate next,
            IOptions<RateLimitOptions> options,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var clientStats = GetClientStatistics(clientId);

            if (IsRateLimitExceeded(clientStats))
            {
                _logger.LogWarning($"Rate limit exceeded for client {clientId}");
                await HandleRateLimitExceeded(context);
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            return $"{context.Connection.RemoteIpAddress}_{context.Request.Headers["User-Agent"]}";
        }
        private ClientStatistics GetClientStatistics(string clientId)
        {
            return _clientStatistics.AddOrUpdate(
                clientId,
                _ => new ClientStatistics
                {
                    LastRequestTime = DateTime.UtcNow,
                    RequestCount = 1
                },
                (_, stats) => UpdateStatistics(stats)
            );
        }
        private ClientStatistics UpdateStatistics(ClientStatistics stats)
        {
            if (DateTime.UtcNow - stats.LastRequestTime >
                TimeSpan.FromMinutes(_options.Value.TimeWindowMinutes))
            {
                stats.RequestCount = 1;
                stats.LastRequestTime = DateTime.UtcNow;
            }
            else
            {
                stats.RequestCount++;
            }
            return stats;
        }
        private bool IsRateLimitExceeded(ClientStatistics stats)
        {
            return stats.RequestCount > _options.Value.MaxRequests;
        }
        private async Task HandleRateLimitExceeded(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse(
                StatusCodes.Status429TooManyRequests,
                "Too many requests. Please try again later."
            );

            await context.Response.WriteAsJsonAsync(response);
        }
        private void CleanupOldEntries()
        {
            var expirationTime = TimeSpan.FromMinutes(_options.Value.TimeWindowMinutes * 2);

            foreach (var key in _clientStatistics.Keys)
            {
                if (_clientStatistics.TryGetValue(key, out var stats))
                {
                    if (DateTime.UtcNow - stats.LastRequestTime > expirationTime)
                    {
                        _clientStatistics.TryRemove(key, out _);
                    }
                }
            }
        }
    }
}