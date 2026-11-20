using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.Entities;

namespace WebAPI.Data.Repository;

public class Repository<TEntity>(WebAPIContext context) : IRepository<TEntity> where TEntity : EntityBase
{
    protected WebAPIContext Context { get; } = context;

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public IQueryable<TEntity> Query() => Set.AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default) =>
        (predicate is null ? Set : Set.Where(predicate)).ToListAsync(cancellationToken);

    public void Add(TEntity entity) => Set.Add(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);
}
