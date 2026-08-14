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

## Tenants

Base path: `/tenants`. All endpoints require `Authorization: Bearer <accessToken>` except accepting
an invitation (the invitee doesn't have an account yet).

### `GET /tenants/me`

Returns the authenticated user's own organization: `{ id, name, slug, subscriptionTier }`.

### `POST /tenants/invitations`

Invites a member by email. Requires the caller's role (from the JWT) to be `Owner` or `Admin` —
otherwise `403`.

```json
{ "email": "new.member@acme.com", "role": "Member" }
```

Returns the invitation: `{ id, tenantId, email, role, accepted, expiresAt }`.

### `GET /tenants/invitations`

Lists all invitations (pending and accepted) for the caller's tenant.

### `POST /tenants/invitations/{invitationId}/accept`

Public — creates the invited user's account and returns a token pair, the same shape as
`/auth/register`. `400` if the invitation doesn't exist, was already accepted, has expired, or an
account with that email already exists.

```json
{ "password": "Password1", "firstName": "Grace", "lastName": "Hopper" }
```

## Projects

Base path: `/projects`. All endpoints require auth and are scoped to the caller's tenant.

- `GET /projects` — list the tenant's projects.
- `POST /projects` — `{ "name": "...", "description": "..." }` → `ProjectDto`.
- `GET /projects/{projectId}` — a single project, including its milestones.
- `PATCH /projects/{projectId}/status` — `{ "status": "Active" }` (`Planning`, `Active`, `OnHold`,
  `Completed`, `Archived`).
- `POST /projects/{projectId}/milestones` — `{ "name": "...", "dueDate": "2026-09-01" }`.
- `GET /projects/{projectId}/tasks` — list the project's tasks.
- `POST /projects/{projectId}/tasks` — `{ "title": "...", "description": "...", "priority": "High",
  "dueDate": null }` (`Low`, `Medium`, `High`, `Critical`).

## Tasks

Base path: `/tasks`. All endpoints require auth and are scoped to the caller's tenant.

- `POST /tasks/{taskId}/assign` — `{ "assigneeId": "..." }`; `400` if the assignee isn't a member
  of the same organization.
- `PATCH /tasks/{taskId}/status` — `{ "status": "InProgress" }` (`Todo`, `InProgress`, `InReview`,
  `Done`). Raises `TaskStatusChangedEvent` (see ADR-002) on change.

## Billing

Base path: `/billing`. All endpoints require auth.

- `GET /billing/plans` — the fixed set of subscribable plans (seeded at startup — see
  `PlanSeeder`).
- `GET /billing/subscription` — the caller's tenant's current subscription, or `null` if none.
- `POST /billing/subscription` — `{ "planId": "..." }`; `400` if the tenant already has a
  non-canceled subscription.
- `POST /billing/subscription/cancel` — cancels the tenant's active subscription.

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

`403` is used specifically for authenticated-but-not-permitted actions (e.g. inviting a member
without Owner/Admin role) — distinct from `400` (business rule violation) and `401`
(unauthenticated).

## Rate limiting

100 requests/minute per tenant (or per IP for unauthenticated requests), sliding window. Exceeding
it returns `429` with `{ "title": "Rate limit exceeded. Try again shortly." }`. See ADR-004.
