using Booking.Application.Abstractions;
using StackExchange.Redis;

namespace Booking.Infrastructure.Locking;

/// <summary>
/// Single-node Redis lock using SET key token NX PX. Release is guarded by a Lua script so a
/// caller only deletes the key while it still owns it (token match), avoiding deleting a lock
/// that has expired and been re-acquired by someone else.
/// </summary>
public class RedisDistributedLock : IDistributedLock
{
    private const string ReleaseScript =
        "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";

    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLock(IConnectionMultiplexer redis) => _redis = redis;

    public async Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var token = Guid.NewGuid().ToString("N");

        var acquired = await db.StringSetAsync(key, token, expiry, When.NotExists);
        return acquired ? new Handle(db, key, token) : null;
    }

    private sealed class Handle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _token;

        public Handle(IDatabase db, string key, string token)
        {
            _db = db;
            _key = key;
            _token = token;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _db.ScriptEvaluateAsync(
                    ReleaseScript,
                    new RedisKey[] { _key },
                    new RedisValue[] { _token });
            }
            catch
            {
                // Best-effort release; the key carries a TTL so it will free itself regardless.
            }
        }
    }
}
