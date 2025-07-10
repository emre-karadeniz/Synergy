using System.Data;

namespace Synergy.Framework.Dapper.UnitOfWork;

public interface IDapperUnitOfWork : IAsyncDisposable, IDisposable
{
    IDbTransaction BeginTransaction();
    Task CommitAsync();
    Task RollbackAsync();
}
