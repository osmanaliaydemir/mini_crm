using System.Collections.Concurrent;
using System.Net;

namespace CRM.Web.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    // IP bazlı rate limiting için in-memory cache
    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitCache = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitingOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Rate limiting sadece belirli endpoint'ler için aktif
        if (!ShouldApplyRateLimit(context))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context);
        var endpoint = context.Request.Path.Value ?? "";

        var cacheKey = $"{clientIp}:{endpoint}";
        var now = DateTime.UtcNow;

        // Eski kayıtları temizle
        CleanupExpiredEntries(now);

        var rateLimitInfo = _rateLimitCache.GetOrAdd(cacheKey, _ => new RateLimitInfo
        {
            FirstRequest = now,
            RequestCount = 0
        });

        // Rate limit kontrolü - Thread-safe
        var shouldBlock = false;
        var currentCount = 0;

        lock (rateLimitInfo)
        {
            // Zaman penceresi dolmuşsa sıfırla
            if (now - rateLimitInfo.FirstRequest > _options.Window)
            {
                rateLimitInfo.FirstRequest = now;
                rateLimitInfo.RequestCount = 0;
            }

            rateLimitInfo.RequestCount++;
            currentCount = rateLimitInfo.RequestCount;

            // Rate limit aşıldı mı?
            if (rateLimitInfo.RequestCount > _options.MaxRequests)
            {
                shouldBlock = true;
            }
        }

        if (shouldBlock)
        {
            _logger.LogWarning(
                "Rate limit exceeded for IP {ClientIp} on endpoint {Endpoint}. Requests: {RequestCount}/{MaxRequests}",
                clientIp, endpoint, currentCount, _options.MaxRequests);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Append("Retry-After",
                ((int)_options.Window.TotalSeconds).ToString());
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Too Many Requests",
                message = $"Rate limit exceeded. Maximum {_options.MaxRequests} requests per {_options.Window.TotalSeconds} seconds.",
                retryAfter = (int)_options.Window.TotalSeconds
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        await _next(context);
    }

    private bool ShouldApplyRateLimit(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Login ve API endpoint'leri için rate limiting
        return path.StartsWith("/Auth/Login", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/Auth/ForgotPassword", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // X-Forwarded-For header'ını kontrol et (reverse proxy arkasında)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // X-Real-IP header'ını kontrol et
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.Trim();
        }

        // Direkt IP adresi
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void CleanupExpiredEntries(DateTime now)
    {
        // Her 100 istekte bir temizlik yap (performans için)
        if (_rateLimitCache.Count % 100 == 0)
        {
            var expiredKeys = _rateLimitCache
                .Where(kvp => now - kvp.Value.FirstRequest > _options.Window.Add(TimeSpan.FromMinutes(5)))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _rateLimitCache.TryRemove(key, out _);
            }
        }
    }

    private class RateLimitInfo
    {
        public DateTime FirstRequest { get; set; }
        public int RequestCount { get; set; }
    }
}

public class RateLimitingOptions
{
    public int MaxRequests { get; set; } = 10; // Varsayılan: 10 istek
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1); // Varsayılan: 1 dakika
}

