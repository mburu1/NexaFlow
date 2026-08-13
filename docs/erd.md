# Entity relationship diagram (SQL Server)

Generated from the actual EF Core migration
(`backend/src/NexaFlow.Infrastructure/Migrations/*_InitialCreate.cs`).

```mermaid
erDiagram
    Tenants ||--o{ Users : "has"
    Tenants ||--o{ Workflows : "has"
    Users ||--o{ RefreshTokens : "has"
    Users ||--o{ WorkflowTasks : "assigned to"
    Users ||--o{ Workflows : "created by"
    Workflows ||--o{ WorkflowTasks : "contains"

    Tenants {
        uniqueidentifier Id PK
        nvarchar Name
        nvarchar Slug UK
        bit IsActive
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    Users {
        uniqueidentifier Id PK
        uniqueidentifier TenantId FK
        nvarchar Email UK
        nvarchar PasswordHash
        nvarchar FullName
        nvarchar Role
        bit IsActive
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    RefreshTokens {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        nvarchar TokenHash UK
        datetime2 ExpiresAtUtc
        datetime2 RevokedAtUtc
        nvarchar ReplacedByTokenHash
        datetime2 CreatedAtUtc
    }

    Workflows {
        uniqueidentifier Id PK
        uniqueidentifier TenantId FK
        uniqueidentifier CreatedByUserId FK
        nvarchar Name
        nvarchar Description
        nvarchar Status
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    WorkflowTasks {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        uniqueidentifier WorkflowId FK
        uniqueidentifier AssignedToUserId FK
        nvarchar Title
        nvarchar Description
        nvarchar Status
        datetime2 DueAtUtc
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    Notifications {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        uniqueidentifier UserId FK
        nvarchar Title
        nvarchar Message
        bit IsRead
        datetime2 ReadAtUtc
    }

    AuditLogs {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        uniqueidentifier UserId
        nvarchar Action
        nvarchar EntityType
        uniqueidentifier EntityId
        nvarchar DataJson
    }
```

Notes:

- `WorkflowTasks.WorkflowId → Workflows` is `ON DELETE NO ACTION` (not cascade) —
  SQL Server rejects the cascade path that would otherwise fan back in through
  `Tenants → Users → WorkflowTasks.AssignedToUserId`. `WorkflowService.DeleteAsync`
  deletes a workflow's tasks explicitly before deleting the workflow itself.
- `WorkflowTasks.AssignedToUserId → Users` is `ON DELETE SET NULL`.
- `Users.Email` and `RefreshTokens.TokenHash` are unique indexes — email is globally
  unique across tenants (see [ADR-003](adr/003-auth-strategy.md)).
- `Notifications` and `AuditLogs` exist in the schema but nothing writes to them yet
  (Phase 2 — see [ADR-002](adr/002-messaging-choice.md)).
