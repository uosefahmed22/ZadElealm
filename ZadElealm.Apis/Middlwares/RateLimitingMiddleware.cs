using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, ClientStatistics> _clientStatistics =
            new ConcurrentDictionary<string, ClientStatistics>();

        public RateLimitingMiddleware(
            RequestDelegate next,
            IOptions<RateLimitOptions> options,
            ILogger<RateLimitingMiddleware> logger,
            IMemoryCache cache)
        {
            _next = next;
            _options = options;
            _logger = logger;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var clientStats = GetClientStatistics(clientId);

            if (IsRateLimitExceeded(clientStats))
            {
                _logger.LogWarning($"Rate limit exceeded for client {clientId}");
                AddRateLimitHeaders(context, clientStats);
                await HandleRateLimitExceeded(context);
                return;
            }

            AddRateLimitHeaders(context, clientStats);
            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            return $"{ipAddress}_{userAgent}";
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
            var timeWindow = TimeSpan.FromMinutes(_options.Value.TimeWindowMinutes);
            var currentTime = DateTime.UtcNow;

            if (currentTime - stats.LastRequestTime > timeWindow)
            {
                stats.RequestCount = 1;
                stats.LastRequestTime = currentTime;
            }
            else
            {
                stats.RequestCount++;
            }

            return stats;
        }

        private bool IsRateLimitExceeded(ClientStatistics stats)
        {
            var timeWindow = TimeSpan.FromMinutes(_options.Value.TimeWindowMinutes);
            return stats.RequestCount > _options.Value.MaxRequests &&
                   DateTime.UtcNow - stats.LastRequestTime <= timeWindow;
        }

        private void AddRateLimitHeaders(HttpContext context, ClientStatistics stats)
        {
            var remainingRequests = Math.Max(0, _options.Value.MaxRequests - stats.RequestCount);
            var resetTime = stats.LastRequestTime.AddMinutes(_options.Value.TimeWindowMinutes);

            context.Response.Headers["X-Rate-Limit-Limit"] = _options.Value.MaxRequests.ToString();
            context.Response.Headers["X-Rate-Limit-Remaining"] = remainingRequests.ToString();
            context.Response.Headers["X-Rate-Limit-Reset"] = resetTime.ToString("O");
        }

        private async Task HandleRateLimitExceeded(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse(
                StatusCodes.Status429TooManyRequests,
                $"Rate limit exceeded. Try again after {_options.Value.TimeWindowMinutes} minute(s)."
            );

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    public class RateLimitCleanupService : BackgroundService
    {
        private readonly ConcurrentDictionary<string, ClientStatistics> _clientStatistics;
        private readonly IOptions<RateLimitOptions> _options;
        private readonly ILogger<RateLimitCleanupService> _logger;

        public RateLimitCleanupService(
            ConcurrentDictionary<string, ClientStatistics> clientStatistics,
            IOptions<RateLimitOptions> options,
            ILogger<RateLimitCleanupService> logger)
        {
            _clientStatistics = clientStatistics;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                CleanupOldEntries();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private void CleanupOldEntries()
        {
            var expirationTime = TimeSpan.FromMinutes(_options.Value.TimeWindowMinutes * 2);
            var currentTime = DateTime.UtcNow;

            foreach (var key in _clientStatistics.Keys)
            {
                if (_clientStatistics.TryGetValue(key, out var stats))
                {
                    if (currentTime - stats.LastRequestTime > expirationTime)
                    {
                        _clientStatistics.TryRemove(key, out _);
                        _logger.LogInformation($"Removed expired rate limit entry for client: {key}");
                    }
                }
            }
        }
    }
}