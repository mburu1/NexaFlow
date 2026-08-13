using Microsoft.EntityFrameworkCore;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Domain.Common;
using NexaFlow.Domain.Entities;

namespace NexaFlow.Infrastructure.Persistence;

/// <summary>
/// SQL Server is the primary relational store for users, tenants, workflows, and
/// audit logs (see ADR-001). Postgres/MySQL/Mongo connection strings exist in
/// configuration for future phases but nothing here talks to them yet.
/// </summary>
public class NexaFlowDbContext(DbContextOptions<NexaFlowDbContext> options, ICurrentUserService currentUserService)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexaFlowDbContext).Assembly);

        // Defense-in-depth tenant isolation: every ITenantScoped entity is filtered to the
        // caller's tenant. When there's no authenticated caller (registration, login lookup
        // by email) TenantId is null, and the filter is a no-op rather than "match nothing" —
        // see ICurrentUserService and ADR-003.
        modelBuilder.Entity<User>().HasQueryFilter(e =>
            !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId);
        modelBuilder.Entity<Workflow>().HasQueryFilter(e =>
            !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId);
        modelBuilder.Entity<WorkflowTask>().HasQueryFilter(e =>
            !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId);
        modelBuilder.Entity<Notification>().HasQueryFilter(e =>
            !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e =>
            !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}
