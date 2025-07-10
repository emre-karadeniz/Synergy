using System.Data;

namespace Synergy.Framework.Dapper.Connections;

public interface IDbConnectionFactory
{
    IDbConnection CreateReadConnection();
    IDbConnection CreateWriteConnection();
}
