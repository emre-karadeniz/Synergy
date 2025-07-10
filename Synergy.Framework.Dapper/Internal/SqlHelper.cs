using Dapper;
using System.Linq.Expressions;
using System.Text;

namespace Synergy.Framework.Dapper.Internal;

internal static class SqlHelper
{
    public static string GetTableName(Type t) => t.Name; // override if you add [Table]

    public static (string Sql, DynamicParameters Parameters) BuildWhere<TEntity>(Expression<Func<TEntity, bool>> expr, bool firstCondition = true)
    {
        // Very naive translator: supports binary equals and AND combinations only
        var parameters = new DynamicParameters();
        var sql = new StringBuilder();

        void Visit(Expression e)
        {
            if (e is BinaryExpression be && be.NodeType == ExpressionType.Equal)
            {
                var member = (MemberExpression)be.Left;
                var constant = (ConstantExpression)be.Right;
                var paramName = $"@p{parameters.ParameterNames.Count()}";
                if (!firstCondition || sql.Length > 0) sql.Append(" AND ");
                sql.Append($"[{member.Member.Name}] = {paramName}");
                parameters.Add(paramName.Trim('@'), constant.Value);
            }
            else if (e is BinaryExpression { NodeType: ExpressionType.AndAlso } andAlso)
            {
                Visit(andAlso.Left);
                Visit(andAlso.Right);
            }
            else
                throw new NotSupportedException("Only simple AND/== expressions are supported in WhereAsync");
        }
        Visit(expr.Body);
        return (sql.ToString(), parameters);
    }
}
