using System.Collections.Concurrent;
using NexaFlow.Domain.Common;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Infrastructure.Persistence.Repositories;

public class UnitOfWork(NexaFlowDbContext dbContext) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : BaseEntity =>
        (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(dbContext));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
