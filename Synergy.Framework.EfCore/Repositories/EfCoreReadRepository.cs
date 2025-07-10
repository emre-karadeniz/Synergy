using Microsoft.EntityFrameworkCore;
using Synergy.Framework.Domain.Interfaces;
using Synergy.Framework.EfCore.Context;
using System.Linq.Expressions;

namespace Synergy.Framework.EfCore.Repositories;

internal class EfCoreReadRepository<TEntity, TId, TDbContext> : IEfCoreReadRepository<TEntity, TId>
where TEntity : class, IEntity, IEntity<TId>
where TId : struct
where TDbContext : BaseDbContext
{
    protected readonly TDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    public EfCoreReadRepository(TDbContext dbContext)
    {
        Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = Context.Set<TEntity>();
    }

    public IQueryable<TEntity> GetAll(bool includeSoftDeleted = false)
    {
        var query = DbSet.AsNoTracking();

        if (includeSoftDeleted)
            query = query.IgnoreQueryFilters();

        return query;
    }

    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate, bool includeSoftDeleted = false)
    {
        var query = DbSet.Where(predicate).AsNoTracking();

        if (includeSoftDeleted)
            query = query.IgnoreQueryFilters();

        return query;
    }

    public IQueryable<TEntity> GetByIdAsQueryable(TId id, bool includeSoftDeleted = false)
    {
        var query = DbSet.Where(e => e.Id.Equals(id)).AsNoTracking();

        if (includeSoftDeleted)
            query = query.IgnoreQueryFilters();

        return query;
    }

    public IQueryable<TEntity> GetByIdsAsQueryable(IEnumerable<TId> ids, bool includeSoftDeleted = false)
    {
        var query = DbSet.Where(e => ids.Contains(e.Id)).AsNoTracking();

        if (includeSoftDeleted)
            query = query.IgnoreQueryFilters();

        return query;
    }

    public async Task<bool> AnyAsync(TId id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().AnyAsync(e => e.Id.Equals(id), cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    => await DbSet.CountAsync(predicate ?? (_ => true), cancellationToken);
}
