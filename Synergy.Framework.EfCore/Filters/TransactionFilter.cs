using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Synergy.Framework.EfCore.Context;
using Synergy.Framework.EfCore.UnitOfWork;

namespace Synergy.Framework.EfCore.Filters;

internal class TransactionFilter : IAsyncActionFilter
{
    private readonly IEnumerable<IEfCoreUnitOfWork<BaseDbContext>> _efUnitOfWorks;

    public TransactionFilter(IEnumerable<IEfCoreUnitOfWork<BaseDbContext>> efUnitOfWorks)
    {
        _efUnitOfWorks = efUnitOfWorks;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpMethod = context.HttpContext.Request.Method.ToUpperInvariant();

        // Sadece yazma işlemlerinde transaction başlat
        if (httpMethod == HttpMethods.Get || httpMethod == HttpMethods.Head || httpMethod == HttpMethods.Options || httpMethod == HttpMethods.Trace)
        {
            await next(); // Hiçbir işlem yapmadan devam et
            return;
        }

        try
        {
            // 1. EF Core transaction'ları başlat
            foreach (var ef in _efUnitOfWorks)
            {
                await ef.BeginTransactionAsync();
            }

            var resultContext = await next();

            if (resultContext.Exception == null || resultContext.ExceptionHandled)
            {
                // 2. Başarılıysa commit
                foreach (var ef in _efUnitOfWorks)
                {
                    await ef.CommitAsync();
                }

            }
            else
            {
                // 3. Hata varsa rollback
                await RollbackAllAsync();
            }
        }
        catch (Exception ex)
        {
            await RollbackAllAsync();
            throw;
        }
    }

    private async Task RollbackAllAsync()
    {
        foreach (var ef in _efUnitOfWorks)
        {
            await ef.RollbackAsync();
        }
    }
}
