namespace EnterpriseHub.Infrastructure.Cache;

public interface ITenantRateLimiter
{
    /// <summary>Redis sorted-set sliding window: true if the caller is still under <paramref name="limit"/> requests within <paramref name="window"/>.</summary>
    Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window, CancellationToken ct = default);
}
