using System.Text.Json;
using Catalog.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Caching;

/// <summary>
/// Cache-aside implementation over <see cref="IDistributedCache"/> (Redis in production,
/// in-memory in local/dev/test). Cache failures never break the request: on any cache
/// error we log and fall back to the underlying data source.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromSeconds(60);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;

    public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default) where T : class
    {
        // 1. Try to read from cache.
        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            if (cached is not null)
                return JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for key {CacheKey}; falling back to source", key);
        }

        // 2. Miss (or cache unavailable): load from the source.
        var value = await factory(ct);

        // 3. Populate the cache for next time (skip negative results).
        if (value is not null)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(value, JsonOptions);
                await _cache.SetStringAsync(
                    key,
                    serialized,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for key {CacheKey}", key);
            }
        }

        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache eviction failed for key {CacheKey}", key);
        }
    }
}
