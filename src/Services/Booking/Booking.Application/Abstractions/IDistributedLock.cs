namespace Booking.Application.Abstractions;

/// <summary>
/// Non-blocking distributed mutex. Implementations acquire the lock if it is free and
/// return a handle that releases it on dispose, or <c>null</c> if it is already held.
/// Backed by Redis in production (port lives here; implementation in Infrastructure).
/// </summary>
public interface IDistributedLock
{
    Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default);
}
