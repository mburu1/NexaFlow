# ADR-002: RabbitMQ for task events, Kafka for the audit stream

## Status

Accepted, not yet implemented. `NexaFlow.Messaging` has `RabbitMqOptions`/`KafkaOptions`
config classes and a single `IEventPublisher` interface backed by `NoOpEventPublisher` —
no producer/consumer code exists yet. This ADR records the decision for when Phase 2
starts.

## Context

Two distinct messaging needs surfaced in the project brief:

1. **Task-assignment → email notification** — low-volume, one message per task
   assignment, needs flexible routing (different notification channels per event type
   later), and simple retry/dead-lettering semantics matter more than raw throughput.
2. **Audit event streaming** — potentially high-volume (every mutating action across every
   tenant), append-only, consumed by at least one durable sink (MongoDB), and would
   benefit from replay/ordering guarantees per entity if consumers are added later.

## Decision

Use **RabbitMQ** for (1) and **Apache Kafka** for (2), rather than picking one broker for
both:

- RabbitMQ's exchange/queue routing and per-message ack/retry model fits a fan-out
  "task assigned → notify" pattern well, and it's simple to reason about for low-volume
  transactional messaging.
- Kafka's log-based, partitioned, high-throughput design fits an append-only audit stream
  that may eventually feed multiple consumers (Mongo persistence today, maybe analytics or
  search indexing later) without re-querying the source of truth.

Using both is a deliberate polyglot-messaging choice, not indecision — it mirrors the
polyglot-persistence choice in [ADR-001](001-database-strategy.md): pick the tool suited to
the workload rather than forcing one broker to do both jobs.

## Consequences

- `IEventPublisher` (in `NexaFlow.Messaging`) is broker-agnostic at the interface level;
  Phase 2 adds a `RabbitMqEventPublisher` and a separate Kafka producer path rather than
  routing everything through one implementation.
- Two brokers means two things to run locally — both are already in `docker-compose.yml`
  (RabbitMQ with management UI on `:15672`, Kafka in single-node KRaft mode on `:9092`) so
  this cost is paid once, in Phase 1, rather than deferred.
- See [docs/sequence-diagrams/task-assignment.md](../sequence-diagrams/task-assignment.md)
  and [docs/sequence-diagrams/kafka-audit-stream.md](../sequence-diagrams/kafka-audit-stream.md)
  for the intended flows.
