# EnterpriseHub

A multi-tenant SaaS platform for team and project management — a Jira + Slack hybrid built from
scratch with deliberate architectural decisions at every layer: Clean Architecture, polyglot
persistence across six databases, dual messaging (RabbitMQ + Kafka), JWT auth, and a full
containerized CI/CD pipeline.

This project exists to demonstrate engineering breadth end to end — not just working code, but
the reasoning behind it (see `docs/adr/`), the sequence it was built in (see `docs/sdlc.md`), and
real bugs caught by actually running things rather than trusting that they would work.

## Architecture

```
Frontend (SvelteKit) → API (ASP.NET Core) → Application (CQRS) → Domain
                                                  ↑                  ↑
                                          Infrastructure ────────────┘
                                     (MSSQL · PostgreSQL · MySQL · Oracle
                                      · MongoDB · Redis · RabbitMQ · Kafka)
```

Full narrative, bounded contexts, and request-flow walkthrough: **[`docs/architecture.md`](docs/architecture.md)**.
Diagrams (PlantUML, renderable): **[`docs/diagrams/`](docs/diagrams/)**.
Architecture decisions with tradeoffs: **[`docs/adr/`](docs/adr/)**.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C# / .NET 10, ASP.NET Core Web API, EF Core, Scalar (OpenAPI docs) |
| Frontend | SvelteKit 2 (Svelte 5), TypeScript |
| Databases | SQL Server (primary), PostgreSQL (audit), MySQL (billing), MongoDB (notifications), Redis (cache/rate-limit), Oracle (reporting) |
| Messaging | RabbitMQ (operational events), Kafka (audit stream) |
| Auth | JWT access + refresh tokens, BCrypt password hashing |
| Testing | xUnit.v3, NSubstitute, Testcontainers, `WebApplicationFactory` |
| Infra | Docker Compose, Kubernetes, Helm, Terraform (Azure AKS) |
| CI/CD | GitHub Actions — lint/build/test, Docker image publish to GHCR, staged K8s rollout |

## Local setup

Requires Docker, the .NET 10 SDK, and Node.js 22+.

```bash
git clone https://github.com/mburu1/EnterpriseHub.git
cd EnterpriseHub
cp .env.example .env          # fill in real values; see "Ports" below for defaults

docker compose up -d          # brings up all 6 databases + RabbitMQ + Kafka + the API + frontend
```

Or run the API and frontend directly against `docker compose`'s databases:

```bash
dotnet restore backend/EnterpriseHub.slnx
dotnet run --project backend/src/EnterpriseHub.API

cd frontend && cp .env.example .env && npm install && npm run dev
```

### Ports (local dev)

| Service | URL |
|---|---|
| API (HTTP) | http://localhost:5001 |
| API (HTTPS) | https://localhost:7001 |
| API docs (Scalar) | http://localhost:5001/scalar |
| Frontend | http://localhost:5173 |
| SQL Server | localhost:1433 |
| PostgreSQL | localhost:5432 |
| MySQL | localhost:3306 |
| MongoDB | localhost:27017 |
| Redis | localhost:6379 |
| Oracle | localhost:1521 |
| RabbitMQ (AMQP / management UI) | localhost:5672 / http://localhost:15672 |
| Kafka | localhost:9092 |

Connection strings and secrets in `.env.example` / `backend/src/EnterpriseHub.API/appsettings.json`
are placeholders — replace `YOUR_PASSWORD` etc. before using anywhere beyond local dev.

### Tests

```bash
dotnet test backend/EnterpriseHub.slnx     # unit + integration + E2E (needs a running Docker daemon)
cd frontend && npm run lint && npm run check && npm run build
```

## API

- Live docs: `/scalar` (Development environment)
- Static reference: [`docs/api-reference.md`](docs/api-reference.md)
- Postman collection: [`docs/postman/EnterpriseHub.postman_collection.json`](docs/postman/EnterpriseHub.postman_collection.json)
  (paired with `local`/`staging` environments in the same folder)

## CI/CD

| Workflow | Trigger | What it does |
|---|---|---|
| `ci-backend.yml` | push/PR touching `backend/**` | restore, build, format check, run the full test suite (unit + integration + E2E via Testcontainers) |
| `ci-frontend.yml` | push/PR touching `frontend/**` | lint, type-check, build |
| `docker-publish.yml` | push to `main`/`develop` | builds and pushes API + frontend images to GHCR |
| `cd-staging.yml` | after a successful `develop` publish | applies K8s manifests, rolls out to the staging cluster |
| `cd-production.yml` | after a successful `main` publish | applies K8s manifests, rolls out to the production cluster |

The two deploy workflows check for a `KUBE_CONFIG_STAGING`/`KUBE_CONFIG_PRODUCTION` repo secret
and skip gracefully (rather than fail) if it isn't configured, so the pipeline is fully
demonstrable without a live cluster attached.

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for branching model, commit conventions, and how to add
an ADR.

## License

[MIT](LICENSE)
