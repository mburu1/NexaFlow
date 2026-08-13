# ADR-001: Polyglot persistence, SQL Server as the Phase 1 store

## Status

Accepted. SQL Server is implemented and in active use; Postgres/MySQL/MongoDB are
provisioned (connection strings, docker-compose services) but not yet consumed by any code
path.

## Context

The project brief calls for polyglot persistence to demonstrate choosing the right
database per workload:

- **SQL Server** — primary relational store for tenants, users, workflows, and a queryable
  audit-log subset. Strong relational integrity (foreign keys, unique constraints) matters
  most here — this is the system of record.
- **PostgreSQL** — intended as an analytics/reporting store (read-replica-style), separate
  from the transactional primary so reporting queries can't contend with OLTP traffic.
- **MySQL** — intended to be owned exclusively by a future Notifications service, so that
  service can evolve its schema independently of the core domain.
- **MongoDB** — intended for unstructured workflow activity/audit documents, which don't
  fit a rigid relational schema as well as SQL Server's `AuditLogs` table does.

## Decision

Build the full `NexaFlowDbContext` (EF Core, code-first migrations) against SQL Server
only for Phase 1, since that's what auth, RBAC, and workflow/task CRUD actually need.
Provision the other three databases in `docker-compose.yml` and list their connection
strings in `appsettings.json`/`appsettings.Development.json` now, so:

1. The infrastructure exists and is documented before it's needed (no last-minute
   plumbing when Phase 2 arrives).
2. It's honest — nothing in the codebase claims to read/write Postgres, MySQL, or Mongo
   today. Comments in `appsettings.json` and this ADR say so explicitly.

## Consequences

- `NexaFlow.Infrastructure` only references `Microsoft.EntityFrameworkCore.SqlServer` —
  no unused Npgsql/Pomelo.MySql/MongoDB.Driver dependencies bloating the project until
  they're actually needed.
- Phase 2 work items: an EF Core `NexaFlowAnalyticsDbContext` (Postgres) fed by a
  read-model projection, a standalone Notifications service with its own MySQL
  `DbContext`, and a Mongo-backed audit-document store consumed from the Kafka audit
  stream (see [ADR-002](002-messaging-choice.md)).
