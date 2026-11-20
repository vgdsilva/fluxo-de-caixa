using System.Linq.Expressions;
using WebAPI.Data.Entities;

namespace WebAPI.Data.Repository;

/// <summary>Repositório genérico de acesso a dados. Regras de negócio ficam em Core/Repository.</summary>
public interface IRepository<TEntity> where TEntity : EntityBase
{
    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    void Add(TEntity entity);

    void Remove(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
