using Synergy.Framework.Domain.Interfaces;
using System.Linq.Expressions;

namespace Synergy.Framework.EfCore.Repositories;

public interface IEfCoreReadRepository<TEntity, TId>
     where TEntity : class, IEntity<TId>
     where TId : struct
{
    // Queryable Metotlar
    IQueryable<TEntity> GetAll(bool includeSoftDeleted = false);
    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate, bool includeSoftDeleted = false);

    // Tek Entity Okuma Metotları
    IQueryable<TEntity> GetByIdAsQueryable(TId id, bool includeSoftDeleted = false);
    IQueryable<TEntity> GetByIdsAsQueryable(IEnumerable<TId> ids, bool includeSoftDeleted = false);

    // Varlık Kontrol Metotları
    Task<bool> AnyAsync(TId id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
