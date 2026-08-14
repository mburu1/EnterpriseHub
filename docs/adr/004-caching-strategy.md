# ADR-004: Redis for Cache and Sliding-Window Rate Limiting

## Status

Accepted

## Context

Two unrelated needs both want a fast, shared, ephemeral store: (1) caching read-heavy query
results across API instances, and (2) per-tenant API rate limiting that has to be consistent
across the `replicas: 2` backend pods (an in-memory limiter would let each pod give a tenant its
own separate quota, defeating the point).

## Decision

- `ICacheService` (`RedisCacheService`) is a thin JSON-serializing wrapper over
  `StackExchange.Redis` for general-purpose caching, available to any `Application` handler that
  wants it.
- Rate limiting (`ITenantRateLimiter` / `RedisSlidingWindowRateLimiter`) uses a **Redis sorted
  set per key** (`ratelimit:{tenantId-or-ip}`), scored by request timestamp. Each check trims
  entries older than the window, counts what's left, and rejects if over the limit — a true
  sliding window rather than a fixed-window counter that resets abruptly at each boundary.
  `TenantRateLimitingMiddleware` applies this to every request (keyed by the `tenant_id` JWT
  claim, falling back to remote IP for unauthenticated requests), ahead of MVC routing.
- **Resilience**: the rate limiter is resolved from `HttpContext.RequestServices` *inside* the
  middleware body (not as a method-injected parameter — `UseMiddleware<T>` resolves those before
  the method runs, which would attempt the Redis connection before any bypass logic could run) and
  wrapped in a try/catch that fails open with a logged warning. `/health` bypasses rate limiting
  entirely. Both exist because a Redis outage must not turn into a total API outage or a failed
  liveness probe — see the incident this caught during initial container testing, described in
  `docs/sdlc.md`.

## Consequences

- **Pro**: rate limits are enforced consistently regardless of which pod handles a given request.
- **Pro**: fail-open behavior means Redis is a performance/cost-control dependency, not an
  availability dependency — a Redis blip degrades to "no rate limiting" rather than "no API."
- **Con**: fail-open is a deliberate tradeoff — under sustained Redis unavailability, a tenant
  could exceed their intended quota. Given this project's scale, that's acceptable; a stricter
  SLA might choose fail-closed for specific high-cost endpoints instead.
- **Con**: the sliding window's `SortedSetAddAsync` + `KeyExpireAsync` are two round trips (plus
  the trim+count batch) — fine at this scale, but a Lua script (`EVAL`) would make the whole
  operation atomic and reduce round trips if this needed to scale further.
