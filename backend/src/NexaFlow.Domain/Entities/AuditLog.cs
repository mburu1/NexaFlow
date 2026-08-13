using NexaFlow.Domain.Common;

namespace NexaFlow.Domain.Entities;

/// <summary>
/// Relational shadow of the audit trail. The MongoDB store (see ADR-001) is the
/// long-term destination for full activity documents once the Kafka audit stream
/// (Phase 2) exists; this table is the queryable, tenant-scoped subset kept in SQL Server.
/// </summary>
public class AuditLog : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? DataJson { get; set; }
}
