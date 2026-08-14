# Architecture

## Overview

EnterpriseHub is a multi-tenant SaaS platform for team and project management. The backend is a
.NET 10 API following Clean Architecture: `Domain` at the center with no outward dependencies,
`Application` orchestrating use cases against `Domain` interfaces, `Infrastructure` implementing
those interfaces against real databases/brokers, and `API` wiring everything together behind REST
controllers. The frontend is a SvelteKit SPA that talks to the API over HTTP.

```
                        ┌──────────────┐
                        │   Frontend   │  SvelteKit (routes, stores, typed API client)
                        │  (SvelteKit) │
                        └──────┬───────┘
                               │ HTTPS / REST
                        ┌──────▼───────┐
                        │     API      │  Controllers, JWT auth, Scalar docs,
                        │ (ASP.NET)    │  rate-limit middleware, exception handling
                        └──────┬───────┘
                               │ ISender (CQRS dispatch)
                        ┌──────▼───────┐
                        │ Application  │  Commands/Queries/Handlers, FluentValidation,
                        │              │  interfaces for everything Infrastructure owns
                        └──────┬───────┘
                               │ implements
                        ┌──────▼───────┐
                        │    Domain    │  Entities, value objects, domain events,
                        │              │  repository interfaces — zero outward deps
                        └──────▲───────┘
                               │ implements
                        ┌──────┴───────┐
                        │Infrastructure│  EF Core (MSSQL/Postgres/MySQL/Oracle),
                        │              │  MongoDB, Redis, RabbitMQ, Kafka, SMTP, JWT
                        └──────────────┘
```

See `docs/diagrams/component-diagram.puml` for the same picture as a renderable PlantUML diagram,
and `docs/diagrams/class-diagram.puml` for the domain model.

## Bounded contexts

The domain is split into five bounded contexts, each owning its own entities, value objects, and
domain events (`backend/src/EnterpriseHub.Domain/*`):

- **Identity** — `User`, `RefreshToken`, `Email` (value object), `TenantRole`. Registration,
  login, token refresh.
- **Tenants** — `Tenant`, `TenantInvitation`, `SubscriptionTier`. Organization creation and member
  invitations.
- **Projects** — `Project`, `Milestone`, `ProjectTask`. Task assignment and status transitions
  raise domain events consumed by the notification and audit paths.
- **Notifications** — `Notification`, persisted in MongoDB (see ADR-001) rather than the primary
  relational store.
- **Billing** — `Plan`, `Subscription`, `Money` (value object). Persisted in MySQL, isolated from
  the core domain's schema.

Each context's repository interfaces live in `Domain` and are implemented in `Infrastructure` —
`Application` handlers never reference EF Core, MongoDB.Driver, or StackExchange.Redis directly.

## Request flow: how a command reaches the database

1. `AuthController` receives an HTTP request, builds a `Command` or `Query` record
   (`Application/Identity/Commands|Queries`), and hands it to `ISender.Send(...)`.
2. `Dispatcher` (the hand-rolled in-process mediator in `Application/Common/Messaging`) resolves
   any registered `IValidator<T>` for the request and runs it first — a failed validation throws
   `FluentValidation.ValidationException`, caught by `GlobalExceptionHandler` and turned into a
   400 with per-field errors.
3. `Dispatcher` resolves the matching `ICommandHandler`/`IQueryHandler` from DI and invokes it.
4. The handler talks to `Domain` repository interfaces and calls domain methods (e.g.
   `User.Register(...)`, `task.ChangeStatus(...)`) — all invariants and domain events live on the
   aggregate, not in the handler.
5. `IUnitOfWork.SaveChangesAsync` commits the EF Core change tracker, then walks tracked
   aggregates for any `IDomainEvent`s raised during the handler and publishes each one to both
   RabbitMQ and Kafka (see ADR-002) before clearing them.
6. The handler returns a DTO; the controller wraps it in a `200 OK` (or the exception handler
   turns a thrown `DomainException` into a `400`).

This custom CQRS pipeline exists instead of a third-party mediator library specifically so the
validation-then-dispatch pipeline is visible and auditable in ~40 lines
(`Application/Common/Messaging/Dispatcher.cs`) rather than a black box.

## Cross-cutting concerns

- **Multi-tenancy**: every tenant-owned entity implements `ITenantScoped` (`Guid TenantId`).
  Repository queries that return tenant data take an explicit `tenantId` parameter — there's no
  global query filter yet (a natural next step: an EF Core global query filter driven by
  `ICurrentUserService.TenantId` to make cross-tenant leakage a compile-time-adjacent concern
  rather than a per-query discipline).
- **Rate limiting**: see ADR-004.
- **Observability**: Serilog to console (structured, ready to redirect to any sink), `/health`
  endpoint for K8s liveness/readiness probes, Scalar-generated OpenAPI docs at `/scalar` in
  Development.
- **Testing**: three tiers (`backend/src/EnterpriseHub.Tests/{Unit,Integration,E2E}`) — Unit tests
  domain/application logic with NSubstitute mocks and no I/O; Integration tests exercise a real EF
  Core repository against a Testcontainers-provisioned SQL Server; E2E drives the actual ASP.NET
  Core pipeline (`WebApplicationFactory<Program>`) through `AuthController`'s HTTP endpoints
  against a real SQL Server container, with RabbitMQ/Kafka/SMTP/Redis swapped for no-op fakes so
  the suite only needs Docker, not the full `docker-compose` stack.

## What's built vs. what's deliberately not

Every bounded context is wired end to end (domain → application → infrastructure → API → tests):
Identity/Auth, Tenants (invitations), Projects/Tasks, and Billing all have working command/query
handlers and controllers, backed by real repository implementations and exercised by tests running
against real infrastructure rather than mocks (see Testing, above).

Not yet built, on top of this foundation:

- **Tenant admin beyond inviting members** — no endpoint to change a member's role, deactivate a
  user, or remove them from the tenant (`User.Deactivate()` exists in the domain, unexposed).
- **Notifications API** — the `Notification` aggregate and its MongoDB repository exist (ADR-001),
  and `TaskAssignedEvent`/`TaskStatusChangedEvent` are published to RabbitMQ (ADR-002), but there's
  no consumer yet that turns those events into `Notification` documents, and no
  `GET /notifications` endpoint for the frontend to read them.
- **Oracle reporting** — `ReportSnapshot`/`OracleDbContext` exist with no migration (ADR-001) and
  no report-generation job writes to it yet.
- **Frontend wiring for the new endpoints** — the SvelteKit routes for
  dashboard/projects/notifications/billing/admin are still stubs; only the auth flow
  (`lib/api/client.ts`) is wired to real endpoints.
