using Synergy.Framework.Dapper.Connections;
using System.Data;

namespace Synergy.Framework.Dapper.UnitOfWork;

internal class DapperUnitOfWork(IDbConnectionFactory factory) : IDapperUnitOfWork
{
    private readonly IDbConnection _con = factory.CreateWriteConnection();
    private IDbTransaction? _tx;

    public IDbTransaction BeginTransaction()
    {
        if (_tx != null) throw new InvalidOperationException("Transaction already begun");
        _con.Open();
        _tx = _con.BeginTransaction();
        return _tx;
    }

    public Task CommitAsync()
    {
        _tx?.Commit();
        _con.Close();
        _tx = null;
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        _tx?.Rollback();
        _con.Close();
        _tx = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _tx?.Dispose();
        _con.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _tx?.Dispose();
        _con.Dispose();
    }
}
