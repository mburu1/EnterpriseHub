# ADR-001: Polyglot Persistence Strategy

## Status

Accepted

## Context

EnterpriseHub needs to persist several kinds of data with different shapes, access patterns, and
consistency requirements: tenant/user/project/task records (highly relational, transactional),
analytics/audit trail (append-only, high write volume, queried for dashboards), billing records
(needs to demonstrate a second relational engine for multi-DB fluency), unstructured activity
feeds/attachments (schema-flexible), hot-path cache/session/rate-limit state (ephemeral,
low-latency), and scheduled reports (batch-generated, read-heavy).

## Decision

Use one datastore per access pattern rather than forcing everything into a single database:

| Store | Owns | Why this store |
|---|---|---|
| **SQL Server (MSSQL)** | Tenants, Users, RefreshTokens, Projects, Milestones, ProjectTasks | Primary transactional store — strong relational integrity for the core domain, first-class EF Core support |
| **PostgreSQL** | Audit log (`audit_log`) | Landing zone for Kafka-streamed domain events; Postgres's indexing and JSON support suit append-only analytical queries |
| **MySQL** | Plans, Subscriptions | Billing domain isolated on its own engine — demonstrates the codebase isn't accidentally coupled to one relational provider, and keeps billing schema changes isolated from the core domain's migrations |
| **MongoDB** | Notifications | High write/read volume, loosely structured, no need for joins — a document store fits better than forcing it through EF migrations |
| **Redis** | Cache, rate-limit sliding windows | Sub-millisecond reads for hot paths that would be wasteful to hit a relational store for |
| **Oracle** | Report snapshots | Reporting schema, deliberately isolated from operational stores so heavy report queries can't contend with transactional workloads |

Each store is accessed only through the layer that owns it (`Infrastructure/Persistence/*`,
`Infrastructure/Mongo`, `Infrastructure/Cache`) — the `Domain` and `Application` layers depend only
on repository interfaces, never on a specific provider.

## Consequences

- **Pro**: each store is used for what it's actually good at; failure or slowness in one store
  (e.g. Oracle reporting) doesn't take down the primary transactional path.
- **Pro**: repository interfaces in `Domain` mean a store can be swapped later without touching
  `Application` or `API`.
- **Con**: six datastores is genuinely more operational surface than a single-database app —
  local dev requires `docker-compose up` to bring up the full stack, and each store needs its own
  backup/monitoring story in production. This is an explicit, accepted tradeoff for this project
  given its purpose (see `docs/sdlc.md`), not a recommendation for every SaaS MVP.
- **Con**: no cross-store transactions — a write that spans, say, MSSQL and the Mongo notification
  store cannot be atomic. Mitigated by the domain-event + message-bus pattern (ADR-002): the primary
  write commits first, and downstream stores are updated via at-least-once event delivery.
- **Known limitation**: `Pomelo.EntityFrameworkCore.MySql` had not yet published an EF Core 10
  build at the time this was built (see `NU1608` warning during restore) — the package still works
  correctly against EF Core 10's relational API surface, but this is worth revisiting on Pomelo's
  next release.
