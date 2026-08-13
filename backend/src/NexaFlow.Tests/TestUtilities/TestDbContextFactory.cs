using Microsoft.EntityFrameworkCore;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Infrastructure.Persistence;

namespace NexaFlow.Tests.TestUtilities;

public static class TestDbContextFactory
{
    /// <summary>Pass the same databaseName across instances to share one InMemory store (e.g. seed then query).</summary>
    public static NexaFlowDbContext Create(ICurrentUserService currentUserService, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<NexaFlowDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new NexaFlowDbContext(options, currentUserService);
    }
}
