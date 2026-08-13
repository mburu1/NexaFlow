using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NexaFlow.Domain.Entities;
using NexaFlow.Domain.Enums;
using NexaFlow.Tests.TestUtilities;

namespace NexaFlow.Tests.Infrastructure;

public class TenantQueryFilterTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task QueryFilter_RestrictsToCurrentTenant_WhenAuthenticated()
    {
        var currentUser = new TestCurrentUserService();
        var dbName = Guid.NewGuid().ToString();

        var tenantA = new Tenant { Name = "Tenant A", Slug = "tenant-a" };
        var tenantB = new Tenant { Name = "Tenant B", Slug = "tenant-b" };

        await using (var seedContext = TestDbContextFactory.Create(currentUser, dbName))
        {
            seedContext.Tenants.AddRange(tenantA, tenantB);
            seedContext.Workflows.Add(new Workflow { TenantId = tenantA.Id, Name = "A Workflow", Status = WorkflowStatus.Draft, CreatedByUserId = Guid.NewGuid() });
            seedContext.Workflows.Add(new Workflow { TenantId = tenantB.Id, Name = "B Workflow", Status = WorkflowStatus.Draft, CreatedByUserId = Guid.NewGuid() });
            await seedContext.SaveChangesAsync(Ct);
        }

        currentUser.UserId = Guid.NewGuid();
        currentUser.TenantId = tenantA.Id;

        await using var scopedContext = TestDbContextFactory.Create(currentUser, dbName);
        var visible = await scopedContext.Workflows.ToListAsync(Ct);

        visible.Should().ContainSingle().Which.Name.Should().Be("A Workflow");
    }

    [Fact]
    public async Task QueryFilter_ReturnsAllTenants_WhenUnauthenticated()
    {
        var currentUser = new TestCurrentUserService();
        var dbName = Guid.NewGuid().ToString();

        await using (var seedContext = TestDbContextFactory.Create(currentUser, dbName))
        {
            var tenantA = new Tenant { Name = "Tenant A", Slug = "tenant-a" };
            var tenantB = new Tenant { Name = "Tenant B", Slug = "tenant-b" };
            seedContext.Tenants.AddRange(tenantA, tenantB);
            seedContext.Workflows.Add(new Workflow { TenantId = tenantA.Id, Name = "A Workflow", Status = WorkflowStatus.Draft, CreatedByUserId = Guid.NewGuid() });
            seedContext.Workflows.Add(new Workflow { TenantId = tenantB.Id, Name = "B Workflow", Status = WorkflowStatus.Draft, CreatedByUserId = Guid.NewGuid() });
            await seedContext.SaveChangesAsync(Ct);
        }

        // No authenticated caller (TenantId stays null) -> the filter is a no-op, not "match nothing".
        await using var scopedContext = TestDbContextFactory.Create(currentUser, dbName);
        var visible = await scopedContext.Workflows.ToListAsync(Ct);

        visible.Should().HaveCount(2);
    }
}
