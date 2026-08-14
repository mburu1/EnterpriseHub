# ADR-002: Two Message Buses — RabbitMQ for Events, Kafka for Audit

## Status

Accepted

## Context

Domain aggregates raise events (`TaskAssignedEvent`, `TaskStatusChangedEvent`,
`UserRegisteredEvent`, `TenantCreatedEvent`, ...). Two very different things need to happen with
these events:

1. **Operational reactions** — e.g. "task assigned" should trigger an in-app + email notification
   to the assignee. This needs low-latency, reliable, at-least-once delivery to a small number of
   consumers, and benefits from routing (topic exchanges, retry queues, dead-lettering).
2. **Audit trail** — every domain event should be durably logged for compliance/analytics
   regardless of whether any operational consumer cares about it, and needs to sustain high
   throughput without back-pressuring the operational path.

These are different problems with different natural tools.

## Decision

- **RabbitMQ** (`IEventPublisher` / `RabbitMqEventPublisher`) carries operational events on a topic
  exchange (`enterprisehub.events`), routed by event type name. Consumers (e.g. a future
  notification worker) bind queues to the routing keys they care about.
- **Kafka** (`IAuditEventPublisher` / `KafkaAuditEventPublisher`) receives every domain event
  unconditionally on `enterprisehub.audit-events`, keyed by event type, for high-throughput
  durable storage and later consumption into the PostgreSQL audit log (ADR-001).
- Both publishers are invoked from a single place: `UnitOfWork.SaveChangesAsync` collects
  `IDomainEvent`s from tracked `AggregateRoot` entities *after* the primary database commit
  succeeds, then publishes to both buses. This keeps every command handler free of messaging
  concerns — raising an event is a one-line `Raise(...)` call on the aggregate.

## Consequences

- **Pro**: RabbitMQ and Kafka are each used close to their design center — Rabbit's routing model
  for "who needs to react to this," Kafka's log model for "what happened, durably, at scale."
- **Pro**: adding a new operational consumer (e.g. a Slack integration) means binding a new queue
  to the exchange — no application code changes.
- **Con**: events are published *after* the DB commit, not in the same transaction (no reliable
  outbox pattern implemented yet) — a crash between commit and publish would lose that event. For
  a production system handling this at scale, the next step is a transactional outbox table
  written in the same DB transaction, drained by a background dispatcher.
- **Con**: two message brokers is more infrastructure than most projects need — justified here
  specifically because demonstrating both messaging patterns is part of this project's purpose.
