# Contributing to EnterpriseHub

## Getting started

1. Clone the repo and copy `.env.example` to `.env`, filling in local values.
2. Run `docker compose up -d` to start all backing services (SQL Server, PostgreSQL, MySQL, MongoDB, Redis, Oracle, RabbitMQ, Kafka).
3. Backend: `dotnet restore backend/EnterpriseHub.slnx && dotnet build backend/EnterpriseHub.slnx`
4. Frontend: `cd frontend && npm install && npm run dev`
5. API docs available at `/scalar` once the API is running.

## Branching model

- `main` — production, deploys via `cd-production.yml` on merge.
- `develop` — staging, deploys via `cd-staging.yml` on merge.
- Feature branches: `feature/<short-description>`, opened as PRs against `develop`.

## Commit style

Conventional commits: `feat:`, `fix:`, `chore:`, `docs:`, `test:`, `refactor:`.

## Pull requests

- Use the PR template.
- CI (`ci-backend.yml` / `ci-frontend.yml`) must pass before merge.
- Include tests for new behavior.

## Architecture decisions

Significant technical decisions are recorded in `docs/adr/` as Architecture Decision Records. Add one when introducing a new dependency, database, or cross-cutting pattern.
