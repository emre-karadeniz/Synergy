using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Synergy.Framework.EfCore.Interceptor;

internal class WithNoLockInterceptor : DbCommandInterceptor
{
    private readonly INolockContext _nolockContext;

    public WithNoLockInterceptor(INolockContext nolockContext)
    {
        _nolockContext = nolockContext;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        if (_nolockContext.UseNoLock && command.CommandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandText = ApplyNoLockToQuery(command.CommandText);
        }

        return base.ReaderExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<DbDataReader> result,
    CancellationToken cancellationToken = default)
    {
        if (_nolockContext.UseNoLock && command.CommandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandText = ApplyNoLockToQuery(command.CommandText);
        }

        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private string ApplyNoLockToQuery(string query)
    {
        // FROM veya JOIN ifadelerinde WITH (NOLOCK) ekler
        var pattern = @"(?<table>(FROM|JOIN)\s+\[.*?\]\s+(AS\s+\[.*?\])?)";
        var replaced = Regex.Replace(query, pattern, match =>
        {
            var value = match.Groups["table"].Value;
            if (value.Contains("WITH (NOLOCK)"))
                return value; // Zaten eklenmişse bir daha ekleme

            return $"{value} WITH (NOLOCK)";
        }, RegexOptions.IgnoreCase);

        return replaced;
    }
}
