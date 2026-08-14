# SDLC

How this project moved from idea to a running, tested, CI-green system.

## Requirements

The starting brief (`Instructions.txt`, kept at the repo root as the original source of truth)
specified: a multi-tenant SaaS platform demonstrating OOP/DDD, polyglot persistence across six
databases, dual messaging (RabbitMQ + Kafka), JWT auth with a specific four-endpoint
`AuthController` contract, containerized deployment (Docker/K8s/Helm), IaC (Terraform), and a full
CI/CD pipeline — explicitly as a portfolio piece signaling senior-level engineering breadth, not a
minimum-viable product for real users.

## Design

Sequenced deliberately in dependency order, matching the commit history on `main`:

1. Repo + GitHub remote, folder scaffold, root config (`.gitignore`, `.editorconfig`,
   `.gitattributes`, `docker-compose.yml`).
2. .NET 10 solution: `Domain` → `Application` → `Infrastructure` → `API` → `Tests`, with project
   references wired in that order so a build failure surfaces as early as possible in the
   dependency chain.
3. NuGet packages per layer, resolved to the latest non-vulnerable versions available (see
   `docs/adr/001-database-strategy.md`'s note on the one accepted `NU1608` exception).
4. Domain model for all five bounded contexts (entities, value objects, domain events, repository
   interfaces) — built first and fully, even though only the Identity/Auth vertical slice got
   wired all the way to the API, so the shape of the whole system is visible in one place.
5. Application layer: a hand-rolled CQRS dispatcher (see ADR discussion in
   `docs/architecture.md`) plus the four `AuthController`-required use cases.
6. Infrastructure: EF Core contexts for four relational stores, MongoDB, Redis, RabbitMQ, Kafka,
   SMTP, JWT/BCrypt.
7. API: controllers, JWT bearer auth, Scalar docs, global exception handling, rate limiting.
8. Tests, written and *run* against real infrastructure (Testcontainers, a live Docker daemon) at
   each tier, not just compiled.
9. Infra manifests: docker-compose, Kubernetes, Helm, Terraform.
10. Frontend: SvelteKit scaffold wired to the same auth flow.
11. CI/CD: backend and frontend pipelines, Docker image publishing, staging/production deploy
    workflows.
12. Documentation (this set of files).

## Implementation practices

- **Build in dependency order, verify each layer before moving on.** Every layer was compiled
  (and, once tests existed, tested) before the next layer was built on top of it, rather than
  writing the whole stack and debugging it as one unit at the end.
- **Commit as you go.** Each meaningful unit of work is its own commit on `main` with a message
  explaining the *why*, not just the *what* — visible in `git log`.
- **Don't trust a build that hasn't run.** Several real bugs were only caught by actually running
  things, not by code review:
  - EF Core couldn't translate `u.Email.Value == x` through a `HasConversion` value-object mapping
    — caught by running the Integration test suite against a real SQL Server container, not by
    reading the LINQ.
  - `UseMiddleware<T>` resolves method-injected parameters via DI *before* the method body runs,
    so a Redis-dependent rate limiter middleware crashed every request — including `/health` —
    the instant Redis was unavailable, even though the middleware had an early-return bypass for
    `/health` in its method body. Caught by actually starting the container and curling `/health`
    without Redis running, not by reading the middleware.
  - `dotnet format --verify-no-changes` passed locally (Windows, `core.autocrlf` rewriting files
    to CRLF on checkout) and failed identically on every push to Linux CI (blobs stored as LF).
    Caught by watching the actual GitHub Actions run, not by trusting a green local check.
  - The frontend and backend Docker images both failed their first real `docker build` — the
    frontend needed `PUBLIC_API_BASE_URL` available at build time for SvelteKit's
    `$env/static/public`, and the backend had no `.dockerignore`, so a locally-built `obj/`
    directory (containing Windows-absolute NuGet fallback paths) leaked into the Linux build
    context and broke `dotnet publish --no-restore`. Both caught by running `docker build`
    locally before trusting the CI-only path.

  Each of these is now an ADR footnote or a comment at the fix site, not just a silent diff — the
  reasoning is preserved for whoever reads this next.

## Testing strategy

Three tiers, described in `docs/architecture.md`. The guiding principle: unit tests should never
need Docker, and integration/E2E tests should never mock the thing they exist to verify (a real
SQL Server via Testcontainers, not an in-memory provider that would have silently accepted the
untranslatable `Email.Value` query above).

## Deployment

`docker-publish.yml` builds and pushes both images to GHCR on every push to `main`/`develop`.
`cd-staging.yml`/`cd-production.yml` apply the Kubernetes manifests and roll out the new image tag
— gated behind a `KUBE_CONFIG_*` repo secret that, if unset, causes the workflow to log a warning
and skip the deploy step rather than fail the run, so the pipeline structure is demonstrable
without requiring a live cluster to exist.

## Known gaps / next iteration

Documented explicitly rather than left implicit:

- Project/task/tenant-invitation/billing `Application` handlers and `API` controllers aren't
  built yet — the domain and infrastructure are ready for them (see `docs/architecture.md`).
- No transactional outbox for domain-event publishing (ADR-002).
- No EF Core global query filter for tenant isolation — currently enforced per-query by passing
  `tenantId` explicitly to every tenant-scoped repository method.
- Frontend has the auth flow wired end-to-end; dashboard/projects/notifications/billing/admin
  pages are route stubs pending the corresponding API endpoints.
