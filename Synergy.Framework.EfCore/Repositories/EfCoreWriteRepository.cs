using Microsoft.EntityFrameworkCore;
using Synergy.Framework.Domain.Interfaces;
using Synergy.Framework.EfCore.Context;

namespace Synergy.Framework.EfCore.Repositories;

internal class EfCoreWriteRepository<TEntity, TId, TDbContext> : IEfCoreWriteRepository<TEntity, TId>
where TEntity : class, IEntity, IEntity<TId>
where TId : struct
where TDbContext : BaseDbContext
{
    protected readonly TDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    public EfCoreWriteRepository(TDbContext dbContext)
    {
        Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = Context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => await DbSet.AddRangeAsync(entities, cancellationToken);

    public void Update(TEntity entity)
        => DbSet.Update(entity);

    public void HardDelete(TEntity entity)
        => DbSet.Remove(entity);

    public void HardDeleteRange(IEnumerable<TEntity> entities)
        => DbSet.RemoveRange(entities);

    public async ValueTask<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(new object[] { id }, cancellationToken);

    public async Task<bool> AnyAsync(TId id, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(e => e.Id.Equals(id), cancellationToken);
}
