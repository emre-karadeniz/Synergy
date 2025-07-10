using Microsoft.EntityFrameworkCore;
using Synergy.Framework.Domain.Interfaces;
using System.Linq.Expressions;

namespace Synergy.Framework.EfCore.Context;

/// <summary>
/// Tüm uygulama veritabanı context'leri için temel soyut sınıfı tanımlar.
/// Soft delete query filter'larını ve ortak DbContext yapılandırmalarını içerir.
/// Auditing ve değişiklik kaydetme sorumluluğu Unit of Work'e devredilmiştir.
/// </summary>
public abstract class BaseDbContext: DbContext
{
    /// <summary>
    /// Yeni bir <see cref="BaseDbContext{TUserKey}"/> örneği oluşturur.
    /// </summary>
    /// <param name="options">DbContext için yapılandırma seçenekleri.</param>
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete query filter'ı uygula
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethod = Expression.Property(parameter, nameof(ISoftDelete.Deleted));
                var filter = Expression.Lambda(Expression.Equal(propertyMethod, Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
