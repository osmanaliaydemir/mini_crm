using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Common.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        return _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            _logger.LogDebug("Cache miss for key: {Key}, fetching data", key);
            return await factory();
        })!;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Cache removed for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // MemoryCache doesn't support prefix-based removal out of the box
        // For now, we'll use a simple approach - remove known cache keys
        // In production, consider using IDistributedCache with Redis for proper prefix-based removal
        
        // Bilinen cache key'lerini temizle (prefix ile başlayanlar)
        // MemoryCache prefix-based removal desteklemediği için bilinen key'leri temizliyoruz
        var keysToRemove = new List<string>();

        // Customer dashboard cache'leri için
        if (prefix == CacheKeys.CustomerDashboardPrefix)
        {
            keysToRemove.Add(CacheKeys.CustomerDashboard(null));
            // Search parametreli cache'leri manuel temizleyemeyiz ama ana cache'i temizleriz
            // Not: Production'da Redis kullanıldığında proper prefix removal mümkün
        }

        // Cash transaction dashboard cache'leri için
        if (prefix == CacheKeys.CashTransactionDashboardPrefix)
        {
            keysToRemove.Add(CacheKeys.CashboxQuickSummary);
            // Filter'lı cache'leri manuel temizleyemeyiz ama ana cache'i temizleriz
            // Not: Production'da Redis kullanıldığında proper prefix removal mümkün
        }

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Cache removed for key: {Key} (prefix: {Prefix})", key, prefix);
        }

        return Task.CompletedTask;
    }
}

