using System.Text.Json;
using EnterpriseHub.Application.Common.Interfaces;
using StackExchange.Redis;

namespace EnterpriseHub.Infrastructure.Cache;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private IDatabase Db => redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await Db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default) =>
        Db.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);

    public Task RemoveAsync(string key, CancellationToken ct = default) => Db.KeyDeleteAsync(key);
}
