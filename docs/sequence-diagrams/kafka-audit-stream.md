# Kafka audit stream (planned, Phase 2)

**Not implemented.** The `AuditLogs` SQL Server table exists in the schema (see
[ERD](../erd.md)) but nothing writes to it, and `NexaFlow.Messaging` has no Kafka producer
today — only config classes (`KafkaOptions`) bound from configuration. This diagram
documents the intended flow once audit events are actually raised and a consumer persists
them into MongoDB. See [ADR-001](../adr/001-database-strategy.md) and
[ADR-002](../adr/002-messaging-choice.md).

```mermaid
sequenceDiagram
    participant Svc as Application services (planned)
    participant Producer as Kafka producer (planned)
    participant Topic as Kafka: nexaflow.audit-events
    participant Consumer as Audit consumer (planned)
    participant Mongo as MongoDB (audit documents)
    participant SqlAudit as SQL Server AuditLogs (queryable subset)

    Svc->>Producer: emit AuditEvent (action, entityType, entityId, tenantId, userId)
    Producer->>Topic: publish (high-throughput, at-least-once)
    Topic->>Consumer: consume
    Consumer->>Mongo: insert full activity document
    Consumer->>SqlAudit: insert queryable row (tenant-scoped reporting)
```
