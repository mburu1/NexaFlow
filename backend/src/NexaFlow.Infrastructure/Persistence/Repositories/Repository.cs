using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NexaFlow.Domain.Common;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Infrastructure.Persistence.Repositories;

public class Repository<T>(NexaFlowDbContext dbContext) : IRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _set = dbContext.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _set;
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public IQueryable<T> Query() => _set.AsQueryable();

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
