using Dapper;
using Synergy.Framework.Dapper.Configuration;
using Synergy.Framework.Dapper.Connections;
using Synergy.Framework.Dapper.Internal;
using Synergy.Framework.Domain.Interfaces;
using System.Linq.Expressions;
using System.Text;

namespace Synergy.Framework.Dapper.Repositories;

internal class DapperReadRepository<TEntity, TId>(IDbConnectionFactory factory, DapperModuleOptions opt)
    : IDapperReadRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    private string Table => SqlHelper.GetTableName(typeof(TEntity));

    private string SoftDeleteClause(bool include) => include || !opt.EnableSoftDeleteFilterByDefault ? "" : "WHERE Deleted = 0";

    public async Task<IEnumerable<TEntity>> GetAllAsync(bool includeSoftDeleted = false)
    {
        var sql = $"SELECT * FROM {Table} {SoftDeleteClause(includeSoftDeleted)}";
        using var con = factory.CreateReadConnection();
        return await con.QueryAsync<TEntity>(sql);
    }

    public async Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate, bool includeSoftDeleted = false)
    {
        var (whereSql, p) = SqlHelper.BuildWhere(predicate, firstCondition: !opt.EnableSoftDeleteFilterByDefault || includeSoftDeleted);
        var sql = new StringBuilder($"SELECT * FROM {Table} ");
        if (!includeSoftDeleted && opt.EnableSoftDeleteFilterByDefault)
        {
            sql.Append("WHERE Deleted = 0");
            if (whereSql.Length > 0) sql.Append(" AND ");
        }
        if (whereSql.Length > 0) sql.Append(whereSql);
        using var con = factory.CreateReadConnection();
        return await con.QueryAsync<TEntity>(sql.ToString(), p);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, bool includeSoftDeleted = false)
    {
        var sql = $"SELECT * FROM {Table} WHERE Id = @id" + (includeSoftDeleted || !opt.EnableSoftDeleteFilterByDefault ? string.Empty : " AND Deleted = 0");
        using var con = factory.CreateReadConnection();
        return await con.QuerySingleOrDefaultAsync<TEntity>(sql, new { id });
    }

    public async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, bool includeSoftDeleted = false)
    {
        var sql = $"SELECT * FROM {Table} WHERE Id IN @ids" + (includeSoftDeleted || !opt.EnableSoftDeleteFilterByDefault ? string.Empty : " AND Deleted = 0");
        using var con = factory.CreateReadConnection();
        return await con.QueryAsync<TEntity>(sql, new { ids });
    }

    public async Task<bool> AnyAsync(TId id)
    {
        var sql = $"SELECT 1 FROM {Table} WHERE Id = @id" + (opt.EnableSoftDeleteFilterByDefault ? " AND Deleted = 0" : "");
        using var con = factory.CreateReadConnection();
        return await con.ExecuteScalarAsync<int?>(sql, new { id }) != null;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var (whereSql, p) = SqlHelper.BuildWhere(predicate);
        var sql = $"SELECT 1 FROM {Table} WHERE {whereSql}" + (opt.EnableSoftDeleteFilterByDefault ? " AND Deleted = 0" : "");
        using var con = factory.CreateReadConnection();
        return await con.ExecuteScalarAsync<int?>(sql, p) != null;
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        string sql;
        DynamicParameters p = new();
        if (predicate is null)
        {
            sql = $"SELECT COUNT(1) FROM {Table} {SoftDeleteClause(false)}";
        }
        else
        {
            var (whereSql, dp) = SqlHelper.BuildWhere(predicate);
            sql = $"SELECT COUNT(1) FROM {Table} WHERE {whereSql}" + (opt.EnableSoftDeleteFilterByDefault ? " AND Deleted = 0" : "");
            p = dp;
        }
        using var con = factory.CreateReadConnection();
        return await con.ExecuteScalarAsync<int>(sql, p);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, bool includeSoftDeleted = false)
    {
        var offset = pageIndex * pageSize;
        var baseWhere = includeSoftDeleted || !opt.EnableSoftDeleteFilterByDefault ? "" : "Deleted = 0";
        var sb = new StringBuilder();
        DynamicParameters p = new();
        if (predicate is not null)
        {
            var (w, dp) = SqlHelper.BuildWhere(predicate, firstCondition: baseWhere == "");
            p = dp;
            sb.Append(w);
        }
        var dynamicPart = sb.ToString();
        var whereClause = sb.Length > 0 ? (baseWhere == "" ? dynamicPart : baseWhere + " AND " + sb) : baseWhere;
        var sql = $"SELECT * FROM {Table} {(whereClause != "" ? "WHERE " + whereClause : "")} ORDER BY Id OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY;";
        var countSql = $"SELECT COUNT(1) FROM {Table} {(whereClause != "" ? "WHERE " + whereClause : "")};";
        p.Add("offset", offset);
        p.Add("take", pageSize);
        using var con = factory.CreateReadConnection();
        var items = await con.QueryAsync<TEntity>(sql, p);
        var total = await con.ExecuteScalarAsync<int>(countSql, p);
        return (items, total);
    }
}
