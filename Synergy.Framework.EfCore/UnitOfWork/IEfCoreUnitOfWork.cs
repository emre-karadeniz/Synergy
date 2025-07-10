using Microsoft.EntityFrameworkCore.Storage;
using Synergy.Framework.EfCore.Context;

namespace Synergy.Framework.EfCore.UnitOfWork;

/// <summary>
/// EF Core tabanlı uygulamalar için belirli bir DbContext'e bağlı Unit of Work arayüzü.
/// </summary>
/// <typeparam name="TDbContext">Unit of Work'in bağlı olduğu DbContext tipi.</typeparam>
public interface IEfCoreUnitOfWork<TDbContext> : IDisposable, IAsyncDisposable
    where TDbContext : BaseDbContext
{
    /// <summary>
    /// Bir veritabanı transaction'ı başlatır.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için kullanılan CancellationToken.</param>
    /// <returns>Başlatılan veritabanı transaction'ı.</returns>
    Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Açık bir transaction'ı commit eder.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için kullanılan CancellationToken.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Açık bir transaction'ı geri alır.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için kullanılan CancellationToken.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Beklemedeki değişiklikleri veritabanına kaydeder.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için kullanılan CancellationToken.</param>
    /// <returns>Kaydedilen varlık sayısını döndürür.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
