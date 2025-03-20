using Microsoft.AspNetCore.RateLimiting;
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

        private readonly RateLimitOptions _options;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(
            RequestDelegate next,
            RateLimitOptions options,
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
                await HandleRateLimitExceeded(context, clientStats);
                return;
            }

            AddRateLimitHeaders(context, clientStats);
            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            return $"{context.Connection.RemoteIpAddress}_{context.Request.Headers["User-Agent"]}";
        }

        private bool IsRateLimitExceeded(ClientStatistics clientStats)
        {
            return clientStats.RequestCount > _options.MaxRequests;
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
            if (DateTime.UtcNow - stats.LastRequestTime > TimeSpan.FromMinutes(_options.TimeWindowMinutes))
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

        private void AddRateLimitHeaders(HttpContext context, ClientStatistics stats)
        {
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] =
                Math.Max(0, _options.MaxRequests - stats.RequestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] =
                stats.LastRequestTime.Add(TimeSpan.FromMinutes(_options.TimeWindowMinutes))
                                   .ToString("o");
        }

        private void AddCacheHeaders(HttpContext context)
        {
            context.Response.Headers["Cache-Control"] = "no-store";
            context.Response.Headers["Pragma"] = "no-cache";
        }

        private async Task HandleRateLimitExceeded(HttpContext context, ClientStatistics stats)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            AddRateLimitHeaders(context, stats);
            AddCacheHeaders(context);

            var response = new ApiResponse(
                StatusCodes.Status429TooManyRequests,
                "Rate limit exceeded. Please try again later."
            );

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}