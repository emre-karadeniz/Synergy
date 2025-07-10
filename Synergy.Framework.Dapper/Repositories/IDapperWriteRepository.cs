using Synergy.Framework.Domain.Interfaces;

namespace Synergy.Framework.Dapper.Repositories;

public interface IDapperWriteRepository<TEntity, in TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateAsync(TEntity entity);
    Task HardDeleteAsync(TEntity entity);
    Task HardDeleteRangeAsync(IEnumerable<TEntity> entities);
}
