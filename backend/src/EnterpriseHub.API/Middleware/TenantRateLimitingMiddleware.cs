using EnterpriseHub.Infrastructure.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseHub.API.Middleware;

/// <summary>
/// Per-tenant (falling back to per-IP for unauthenticated requests) request throttling backed by a
/// Redis sliding window (see <see cref="RedisSlidingWindowRateLimiter"/>). Applied ahead of MVC so
/// throttled requests never reach a controller. Skips infrastructure paths like /health so orchestrator
/// probes stay independent of Redis availability, and fails open (with a logged warning) if Redis
/// itself is unreachable rather than 500-ing every request in the cluster.
///
/// Note: ITenantRateLimiter is resolved from context.RequestServices inside InvokeAsync rather than
/// as a method parameter — UseMiddleware&lt;T&gt; resolves method-injected parameters from DI before
/// the method body runs, which would attempt the Redis connection (and throw) before the /health
/// bypass below ever gets a chance to run.
/// </summary>
public sealed class TenantRateLimitingMiddleware(RequestDelegate next, ILogger<TenantRateLimitingMiddleware> logger)
{
    private const int LimitPerWindow = 100;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        var key = tenantId ?? $"ip:{context.Connection.RemoteIpAddress}";

        bool allowed;
        try
        {
            var rateLimiter = context.RequestServices.GetRequiredService<ITenantRateLimiter>();
            allowed = await rateLimiter.IsAllowedAsync(key, LimitPerWindow, Window, context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Rate limiter unavailable; allowing request through for {Key}", key);
            allowed = true;
        }

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { title = "Rate limit exceeded. Try again shortly." });
            return;
        }

        await next(context);
    }
}
