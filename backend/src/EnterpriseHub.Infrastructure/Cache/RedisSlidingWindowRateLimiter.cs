using StackExchange.Redis;

namespace EnterpriseHub.Infrastructure.Cache;

/// <summary>
/// Sliding-window rate limiter backed by a Redis sorted set: each request is a member scored by its
/// timestamp; entries older than the window are trimmed before counting, so the limit is enforced
/// continuously rather than resetting at fixed intervals.
/// </summary>
public sealed class RedisSlidingWindowRateLimiter(IConnectionMultiplexer redis) : ITenantRateLimiter
{
    public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var redisKey = new RedisKey($"ratelimit:{key}");
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)window.TotalMilliseconds;

        var batch = db.CreateBatch();
        var trimTask = batch.SortedSetRemoveRangeByScoreAsync(redisKey, double.NegativeInfinity, windowStart);
        var countTask = batch.SortedSetLengthAsync(redisKey);
        batch.Execute();
        await Task.WhenAll(trimTask, countTask);

        if (await countTask >= limit)
            return false;

        await db.SortedSetAddAsync(redisKey, now.ToString(), now);
        await db.KeyExpireAsync(redisKey, window);
        return true;
    }
}
