using Synergy.Framework.Domain.Interfaces;

namespace Synergy.Framework.EfCore.Repositories;

public interface IEfCoreWriteRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void HardDelete(TEntity entity);
    void HardDeleteRange(IEnumerable<TEntity> entities);
    ValueTask<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(TId id, CancellationToken cancellationToken = default);
}
