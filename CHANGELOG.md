# Changelog

All notable changes to this project are documented here.
Format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added
- Full repository scaffold: solution structure, docker-compose local environment, K8s/Helm/Terraform
  infrastructure, CI/CD workflows, documentation.
- Domain model for all five bounded contexts (Identity, Tenants, Projects, Notifications, Billing) —
  entities, value objects, domain events, repository interfaces.
- `AuthController` with register / login / refresh / me endpoints, JWT access + refresh token flow,
  wired end to end through a hand-rolled CQRS dispatcher with FluentValidation.
- EF Core persistence across MSSQL (primary), PostgreSQL (audit), MySQL (billing), and Oracle
  (reporting); MongoDB for notifications; Redis for caching and a sliding-window rate limiter.
- RabbitMQ (operational events) and Kafka (audit stream) domain-event publishing, dispatched from
  `UnitOfWork.SaveChangesAsync`.
- SvelteKit frontend: auth flow (register/login), typed API client, route stubs for
  dashboard/projects/notifications/billing/admin.
- Three-tier test suite (unit, Testcontainers-backed integration, `WebApplicationFactory` E2E) — 29
  tests, all passing against real infrastructure, not mocks.
- GitHub Actions: `ci-backend`, `ci-frontend`, `docker-publish` (GHCR), `cd-staging`/`cd-production`
  (K8s rollout, gracefully skipped without cluster credentials).
- EF Core migrations for MSSQL and PostgreSQL (see ADR-001 for why MySQL's aren't generated yet).
- Documentation: README, architecture narrative, SDLC writeup, four ADRs, PlantUML diagrams,
  Postman collection, API reference.

### Fixed
- EF Core couldn't translate a LINQ predicate accessing `.Value` through a value-converted property
  (`Email`) — rewritten to compare the value object directly.
- Rate-limiting middleware crashed every request (including `/health`) when Redis was unreachable,
  because `UseMiddleware<T>` resolves method-injected DI parameters before the method body's
  bypass logic runs; fixed by resolving the dependency lazily inside the method and failing open.
- `dotnet format --verify-no-changes` disagreed between local Windows checkouts and Linux CI due to
  a `core.autocrlf`/`.editorconfig` mismatch; added `.gitattributes` to normalize line endings.
- Both Docker images failed their first real build: the frontend needed `PUBLIC_API_BASE_URL` at
  build time, and the backend had no `.dockerignore`, letting a Windows-built `obj/` directory
  leak into the Linux build context.
