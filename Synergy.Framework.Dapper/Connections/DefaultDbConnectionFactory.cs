using Microsoft.Extensions.Configuration;
using Synergy.Framework.Dapper.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Synergy.Framework.Dapper.Connections;

public class DefaultDbConnectionFactory: IDbConnectionFactory
{
    private readonly IConfiguration _cfg;
    private readonly DapperModuleOptions _options;

    public DefaultDbConnectionFactory(IConfiguration cfg, DapperModuleOptions options)
    {
        _cfg = cfg;
        _options = options;
    }

    public IDbConnection CreateReadConnection()
    {
        var cs = _cfg.GetConnectionString(_options.SeparateReadDatabase ? _options.ReadConnectionStringName : _options.ConnectionStringName);
        return new SqlConnection(cs);
    }

    public IDbConnection CreateWriteConnection()
    {
        var cs = _cfg.GetConnectionString(_options.SeparateReadDatabase ? _options.WriteConnectionStringName : _options.ConnectionStringName);
        return new SqlConnection(cs);
    }
}
