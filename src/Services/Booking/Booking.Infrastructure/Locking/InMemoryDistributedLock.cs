using System.Collections.Concurrent;
using Booking.Application.Abstractions;

namespace Booking.Infrastructure.Locking;

/// <summary>
/// In-process fallback used when Redis is not configured (local dev / tests). Provides the same
/// non-blocking try-lock semantics as the Redis implementation, but only within a single process.
/// </summary>
public class InMemoryDistributedLock : IDistributedLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();

    public async Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        var gate = Gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        var acquired = await gate.WaitAsync(TimeSpan.Zero, ct);
        return acquired ? new Handle(gate) : null;
    }

    private sealed class Handle : IAsyncDisposable
    {
        private readonly SemaphoreSlim _gate;
        public Handle(SemaphoreSlim gate) => _gate = gate;

        public ValueTask DisposeAsync()
        {
            _gate.Release();
            return ValueTask.CompletedTask;
        }
    }
}
