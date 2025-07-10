using Synergy.Framework.Domain.Interfaces;
using System.Linq.Expressions;

namespace Synergy.Framework.Dapper.Repositories;

public interface IDapperReadRepository<TEntity, in TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    Task<IEnumerable<TEntity>> GetAllAsync(bool includeSoftDeleted = false);
    Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate, bool includeSoftDeleted = false);

    Task<TEntity?> GetByIdAsync(TId id, bool includeSoftDeleted = false);
    Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, bool includeSoftDeleted = false);

    Task<bool> AnyAsync(TId id);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, bool includeSoftDeleted = false);
}
