using EnterpriseHub.Infrastructure.Cache;

namespace EnterpriseHub.API.Middleware;

/// <summary>
/// Per-tenant (falling back to per-IP for unauthenticated requests) request throttling backed by a
/// Redis sliding window (see <see cref="RedisSlidingWindowRateLimiter"/>). Applied ahead of MVC so
/// throttled requests never reach a controller.
/// </summary>
public sealed class TenantRateLimitingMiddleware(RequestDelegate next)
{
    private const int LimitPerWindow = 100;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(HttpContext context, ITenantRateLimiter rateLimiter)
    {
        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        var key = tenantId ?? $"ip:{context.Connection.RemoteIpAddress}";

        var allowed = await rateLimiter.IsAllowedAsync(key, LimitPerWindow, Window, context.RequestAborted);
        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { title = "Rate limit exceeded. Try again shortly." });
            return;
        }

        await next(context);
    }
}
