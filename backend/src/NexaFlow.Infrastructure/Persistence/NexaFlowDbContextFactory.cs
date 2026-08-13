using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NexaFlow.Application.Common.Interfaces;

namespace NexaFlow.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add` construct NexaFlowDbContext without spinning up the
/// full Api host/DI container. Only used by EF Core design-time tooling, never at runtime.
/// </summary>
public class NexaFlowDbContextFactory : IDesignTimeDbContextFactory<NexaFlowDbContext>
{
    public NexaFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NexaFlowDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=NexaFlow;Trusted_Connection=True;TrustServerCertificate=True");

        return new NexaFlowDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }

    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid? TenantId => null;
        public string? Email => null;
        public string? Role => null;
        public bool IsAuthenticated => false;
    }
}
