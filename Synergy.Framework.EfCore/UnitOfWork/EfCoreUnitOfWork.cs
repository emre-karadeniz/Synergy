using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Synergy.Framework.Domain.Interfaces;
using Synergy.Framework.EfCore.Context;
using Synergy.Framework.Shared.Abstractions;

namespace Synergy.Framework.EfCore.UnitOfWork;

//ÇOK ÖNEMLİ NOT:
//BURADAKİ HANDLER KULLANMAK İÇİN API TARAFINDA 
//services.AddScoped<IAuditLogHandler, DefaultAuditLogHandler>();
//services.AddScoped<IAuditLogHandler, NullAuditLogHandler>(); // hiçbir şey yapmayan boş handler
//services.AddScoped<IUserIdentifierHandler, DefaultUserIdentifierHandler>();
//AYARLANMASI GEREKİYOR. BÖYLECE 2 KÜTÜPHANEYİ ENTEGRE EDİP LOGLAMA İŞLEMİ YAPILABİLİR.

internal class EfCoreUnitOfWork<TDbContext, TUserKey> : IEfCoreUnitOfWork<TDbContext>
    where TDbContext : BaseDbContext
{
    public TDbContext Context { get; }
    private IDbContextTransaction? _currentTransaction;
    private readonly IUserIdentifierHandler<TUserKey> _userIdentifierHandler;
    private readonly IAuditLogHandler _auditLogHandler;

    public EfCoreUnitOfWork(
        TDbContext context,
        IUserIdentifierHandler<TUserKey> userIdentifierHandler,
        IAuditLogHandler? auditLogHandler = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _userIdentifierHandler = userIdentifierHandler;
        _auditLogHandler = auditLogHandler;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PrepareAuditChanges();
        return await Context.SaveChangesAsync(cancellationToken);
    }

    private void PrepareAuditChanges()
    {
        var userId = _userIdentifierHandler.UserId;
        var now = DateTime.Now;

        foreach (var entry in Context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            if (entry.Entity is ICreationAudited<TUserKey> creation && entry.State == EntityState.Added && _auditLogHandler is not null)
            {
                creation.CreatedAt = now;
                creation.CreatedByUserId = userId;
                _auditLogHandler.Add(entry.Entity);
            }

            if (entry.Entity is IModificationAudited<TUserKey> modification && entry.State == EntityState.Modified && _auditLogHandler is not null)
            {
                var oldValues = Activator.CreateInstance(entry.Entity.GetType(), true);
                foreach (var prop in entry.OriginalValues.Properties)
                {
                    prop.PropertyInfo?.SetValue(oldValues, entry.OriginalValues[prop]);
                }

                modification.UpdatedAt = now;
                modification.UpdatedByUserId = userId;
                _auditLogHandler.Update(entry.Entity, oldValues);
            }

            if (entry.Entity is ISoftDelete<TUserKey> softDelete && entry.State == EntityState.Modified && softDelete.IsSoftDeleting && _auditLogHandler is not null)
            {
                softDelete.Deleted = true;
                softDelete.DeletedAt = now;
                softDelete.DeletedByUserId = userId;
                _auditLogHandler.SoftDelete(entry.Entity);
            }

            if (entry.State == EntityState.Deleted && _auditLogHandler is not null)
            {
                _auditLogHandler.HardDelete(entry.Entity);
            }
        }
    }

    public async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Transaction already in progress.");
        }
        _currentTransaction = await Context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to commit.");
        }
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback.");
        }
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public IDbContextTransaction? GetCurrentTransaction() => _currentTransaction;

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
