namespace Catalog.Application.Abstractions;

/// <summary>
/// Cache-aside abstraction. Lives in Application (a port); the Redis-backed
/// implementation lives in Infrastructure so the domain/service logic stays
/// unaware of the caching technology.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or invokes
    /// <paramref name="factory"/> on a miss, caches its non-null result, and returns it.
    /// Implementations must degrade gracefully: if the cache is unavailable the factory
    /// result is still returned.
    /// </summary>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default) where T : class;

    /// <summary>Evicts a key (used by write paths / event consumers to invalidate stale reads).</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);
}
