using Dapper;
using Synergy.Framework.Dapper.Configuration;
using Synergy.Framework.Dapper.Connections;
using Synergy.Framework.Dapper.Internal;
using Synergy.Framework.Domain.Interfaces;
using Synergy.Framework.Shared.Abstractions;

namespace Synergy.Framework.Dapper.Repositories;

internal class DapperWriteRepository<TEntity, TId>(IDbConnectionFactory factory, DapperModuleOptions opt, IAuditLogHandler? audit)
: IDapperWriteRepository<TEntity, TId>
where TEntity : class, IEntity<TId>
where TId : struct
{
    private string Table => SqlHelper.GetTableName(typeof(TEntity));

    public async Task AddAsync(TEntity entity)
    {
        using var con = factory.CreateWriteConnection();
        var sql = GenerateInsertSql(entity, out var parameters);
        await con.ExecuteAsync(sql, parameters);
        audit?.Add(entity);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        using var con = factory.CreateWriteConnection();
        foreach (var e in entities)
        {
            var sql = GenerateInsertSql(e, out var p);
            await con.ExecuteAsync(sql, p);
            audit?.Add(e);
        }
    }

    public async Task UpdateAsync(TEntity entity)
    {
        using var con = factory.CreateWriteConnection();
        var sql = GenerateUpdateSql(entity, out var p);
        await con.ExecuteAsync(sql, p);
        audit?.Update(entity, null!); // oldEntity fetch could be added separately
    }

    public async Task HardDeleteAsync(TEntity entity)
    {
        using var con = factory.CreateWriteConnection();
        await con.ExecuteAsync($"DELETE FROM {Table} WHERE Id=@id", new { id = entity.Id });
        audit?.HardDelete(entity);
    }

    public async Task HardDeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        using var con = factory.CreateWriteConnection();
        await con.ExecuteAsync($"DELETE FROM {Table} WHERE Id IN @ids", new { ids = entities.Select(e => e.Id) });
        foreach (var e in entities) audit?.HardDelete(e);
    }

    // ------- helpers -------
    private static string GenerateInsertSql(TEntity entity, out DynamicParameters p)
    {
        var props = typeof(TEntity).GetProperties().Where(pr => pr.Name != "Id");
        p = new DynamicParameters();
        foreach (var pr in props) p.Add(pr.Name, pr.GetValue(entity));
        var cols = string.Join(",", props.Select(pr => $"[{pr.Name}]"));
        var vals = string.Join(",", props.Select(pr => $"@{pr.Name}"));
        return $"INSERT INTO {SqlHelper.GetTableName(typeof(TEntity))} ({cols}) VALUES ({vals});";
    }

    private static string GenerateUpdateSql(TEntity entity, out DynamicParameters p)
    {
        var props = typeof(TEntity).GetProperties().Where(pr => pr.Name != "Id");
        p = new DynamicParameters();
        var sets = new List<string>();
        foreach (var pr in props)
        {
            var val = pr.GetValue(entity);
            p.Add(pr.Name, val);
            sets.Add($"[{pr.Name}] = @{pr.Name}");
        }
        p.Add("id", entity.Id);
        return $"UPDATE {SqlHelper.GetTableName(typeof(TEntity))} SET {string.Join(",", sets)} WHERE Id=@id";
    }
}
