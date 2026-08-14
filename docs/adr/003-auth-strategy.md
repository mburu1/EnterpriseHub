# ADR-003: JWT Access + Refresh Tokens, Email Globally Unique

## Status

Accepted

## Context

The API needs stateless authentication that scales horizontally without a shared session store,
supports short-lived credentials for security, and works cleanly with an SPA frontend
(SvelteKit) that can't rely on server-rendered session cookies for every route.

## Decision

- **Access tokens** are short-lived (15 min default, `Jwt:AccessTokenMinutes`) signed JWTs
  (HMAC-SHA256) carrying `sub` (user id), `email`, `tenant_id`, and a role claim. They're
  validated by the standard `Microsoft.AspNetCore.Authentication.JwtBearer` middleware — no
  server-side session lookup on every request.
- **Refresh tokens** are opaque random values (not JWTs), stored server-side only as a SHA-256
  hash (`RefreshToken.TokenHash`) with an expiry (7 days default) and revocation state. `POST
  /auth/refresh` rotates the token: the old one is revoked and linked to its replacement
  (`ReplacedByTokenId`), and a new pair is issued. This means a leaked refresh token can be
  revoked, and reuse of an already-rotated token is detectable (a hardening opportunity: reject
  and revoke the whole chain if a revoked token is presented again — not yet implemented).
- **Passwords** are hashed with BCrypt (`EnhancedHashPassword`, work factor 12) — deliberately not
  a fast hash, to make offline brute-force expensive.
- **Email is globally unique** across the whole system (`IUserRepository.ExistsByEmailAsync` has
  no tenant scope), i.e. one user account belongs to exactly one tenant in v1. This is a deliberate
  simplification: real multi-tenant products (Slack, Notion) let one email join multiple
  workspaces, which requires a separate "membership" concept decoupled from the `User` aggregate.
  That's a legitimate future iteration, not implemented here to keep the registration/login flow
  in `AuthController` matching the spec's four endpoints without introducing a workspace-switcher
  concept that nothing else in this codebase depends on yet.
- Registration (`POST /auth/register`) creates a **new tenant and its owner user** in one step —
  the self-serve SaaS signup flow. Joining an *existing* tenant happens via
  `Tenant.InviteMember` → `TenantInvitation` (domain-modeled, not yet wired to an API endpoint).

## Consequences

- **Pro**: no server-side session state for access tokens — any API instance can validate a
  request without a shared cache lookup, which is what makes the K8s `replicas: 2` deployment
  (`infrastructure/k8s/backend-deployment.yml`) safe.
- **Pro**: refresh token rotation limits the blast radius of a stolen refresh token to one
  request's worth of validity.
- **Con**: revoking a *access* token before it expires isn't possible (no blocklist) — 15 minutes
  is the deliberate ceiling on that exposure window. A Redis-backed access-token blocklist would
  close this gap if a tighter bound were needed.
- **Con**: global email uniqueness is a real product constraint that would need to change before
  supporting multi-workspace membership — flagged above rather than hidden.
