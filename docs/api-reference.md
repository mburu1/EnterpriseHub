# API Reference

The source of truth is the live, auto-generated OpenAPI document and Scalar UI: run the API
(`dotnet run --project backend/src/EnterpriseHub.API`) and open
**http://localhost:5001/scalar** (or **https://localhost:7001/scalar**). This file is a static
summary for quick reference and for readers browsing the repo without running it.

## Auth

Base path: `/auth`. None of these endpoints are tenant-scoped by header/query param — the tenant
is derived from the authenticated user's JWT (`tenant_id` claim) or, for registration, created
fresh.

### `POST /auth/register`

Creates a new organization (tenant) and its owner user in one step. Returns a token pair
immediately — no separate login required after signup.

**Request**
```json
{
  "organizationName": "Acme Inc",
  "email": "owner@acme.com",
  "password": "Password1",
  "firstName": "Ada",
  "lastName": "Lovelace"
}
```

**Response `200 OK`**
```json
{
  "accessToken": "eyJhbGciOi...",
  "accessTokenExpiresAt": "2026-08-14T12:15:00Z",
  "refreshToken": "base64-opaque-token",
  "user": {
    "id": "...", "tenantId": "...", "email": "owner@acme.com",
    "firstName": "Ada", "lastName": "Lovelace", "role": "Owner"
  }
}
```

**Errors**: `400` with a `ProblemDetails` body — validation failures (weak password, invalid
email) or a `DomainException` (email already registered).

### `POST /auth/login`

Exchanges credentials for a token pair. Same response shape as register.

```json
{ "email": "owner@acme.com", "password": "Password1" }
```

`400` on invalid credentials (deliberately the same message for "no such user" and "wrong
password", to avoid leaking which emails are registered).

### `POST /auth/refresh`

Rotates a refresh token. The presented token is revoked and linked to its replacement; reusing an
already-rotated or expired token returns `400`.

```json
{ "refreshToken": "base64-opaque-token" }
```

Returns a new token pair with the same shape as register/login.

### `GET /auth/me`

Requires `Authorization: Bearer <accessToken>`. Returns the authenticated user, and exists
specifically to prove the JWT round-trips through `[Authorize]` correctly.

`401` if the token is missing, expired, or invalid.

## Health

### `GET /health`

No auth required, not subject to rate limiting — used by the Kubernetes liveness/readiness probes
in `infrastructure/k8s/backend-deployment.yml`. Returns `200` with an empty body when the process
is up; does not check downstream database/broker health (see ADR-004 for why the rate limiter
specifically bypasses this path).

## Errors

All error responses use RFC 7807 `ProblemDetails`:

```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "instance": "/auth/register",
  "errors": { "Password": ["Password must contain an uppercase letter."] }
}
```

## Rate limiting

100 requests/minute per tenant (or per IP for unauthenticated requests), sliding window. Exceeding
it returns `429` with `{ "title": "Rate limit exceeded. Try again shortly." }`. See ADR-004.
