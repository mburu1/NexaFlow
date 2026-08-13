namespace NexaFlow.Domain.Common;

/// <summary>
/// Marks an entity as belonging to a single tenant. NexaFlowDbContext applies a global
/// query filter on every ITenantScoped entity based on the caller's current tenant claim,
/// so cross-tenant reads are impossible by construction rather than by convention.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
