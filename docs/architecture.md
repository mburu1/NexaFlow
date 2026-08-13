# Architecture overview

## Layering

Clean/onion architecture, dependencies pointing inward:

```
NexaFlow.Domain            entities, enums, repository interfaces — no external deps
   ^
NexaFlow.Application        DTOs, service interfaces + implementations, FluentValidation
   ^
NexaFlow.Infrastructure     EF Core (SQL Server), JWT/BCrypt, repositories
NexaFlow.Messaging          IEventPublisher (NoOp today — Phase 2)
NexaFlow.Notifications      IEmailSender (real SMTP client, unwired — Phase 2)
   ^
NexaFlow.Api                Program.cs wiring, controllers, appsettings
```

`NexaFlow.Tests` references `Domain`/`Application`/`Infrastructure` and runs against EF
Core's InMemory provider — no external services required to run the suite.

## System diagram

Solid lines are implemented and exercised by the test suite / manual smoke tests. Dashed
lines are provisioned (container runs, connection string exists) but not yet consumed by
any code path — see [ADR-001](adr/001-database-strategy.md) and
[ADR-002](adr/002-messaging-choice.md) for why.

```mermaid
flowchart LR
    subgraph Client
        FE["React dashboard\n(shell only — Phase 2)"]
        Postman["Postman / Scalar UI"]
    end

    subgraph API["NexaFlow.Api"]
        Auth["AuthController"]
        CRUD["Tenants/Users/Workflows/\nWorkflowTasks controllers"]
        Health["/health"]
    end

    subgraph App["NexaFlow.Application"]
        Services["AuthService, TenantService,\nUserService, WorkflowService,\nWorkflowTaskService"]
    end

    subgraph Infra["NexaFlow.Infrastructure"]
        DbCtx["NexaFlowDbContext\n(tenant query filter)"]
        Jwt["JwtTokenService / PasswordHasher"]
    end

    MsSql[("SQL Server\nusers, tenants, workflows, audit")]
    Postgres[("PostgreSQL\nanalytics — unused")]
    MySql[("MySQL\nnotifications DB — unused")]
    Mongo[("MongoDB\naudit documents — unused")]
    Redis[("Redis\ncache")]
    RabbitMQ[("RabbitMQ\ntask events — unused")]
    Kafka[("Kafka\naudit stream — unused")]
    Mail["MailHog / SMTP\n(MailKitEmailSender — unwired)"]

    FE -.->|Phase 2| Auth
    Postman --> Auth
    Postman --> CRUD

    Auth --> Services
    CRUD --> Services
    Services --> DbCtx
    Services --> Jwt

    DbCtx --> MsSql
    Infra -.-> Postgres
    Infra -.-> MySql
    Infra -.-> Mongo
    Infra --> Redis

    API -.->|Phase 2| RabbitMQ
    API -.->|Phase 2| Kafka
    API -.->|Phase 2| Mail

    Health --> MsSql
    Health --> Redis
```

## Multi-tenancy

Every tenant-scoped entity (`User`, `Workflow`, `WorkflowTask`, `Notification`,
`AuditLog`) implements `ITenantScoped`. `NexaFlowDbContext` applies an EF Core global
query filter per entity:

```csharp
modelBuilder.Entity<Workflow>().HasQueryFilter(e =>
    !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId);
```

`ICurrentUserService.TenantId` comes from the JWT `tenant_id` claim. When there's no
authenticated caller (registration, login-by-email lookup) the filter is a no-op rather
than "match nothing" — otherwise duplicate-email checks and login could never find an
existing user. See [ADR-003](adr/003-auth-strategy.md) for the full reasoning, and
`NexaFlow.Tests/Infrastructure/TenantQueryFilterTests.cs` for the test that pins this
behavior down.
