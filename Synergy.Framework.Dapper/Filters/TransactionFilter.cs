using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Synergy.Framework.Dapper.UnitOfWork;

namespace Synergy.Framework.Dapper.Filters;

internal class TransactionFilter : IAsyncActionFilter
{
    private readonly IEnumerable<IDapperUnitOfWork> _dapperUnitOfWorks;

    public TransactionFilter(IEnumerable<IDapperUnitOfWork> dapperUnitOfWorks)
    {
        _dapperUnitOfWorks = dapperUnitOfWorks;
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
            // 1. Dapper transaction'ları başlat
            foreach (var dapper in _dapperUnitOfWorks)
            {
                dapper.BeginTransaction();
            }

            var resultContext = await next();

            if (resultContext.Exception == null || resultContext.ExceptionHandled)
            {
                // 2. Başarılıysa commit
                foreach (var dapper in _dapperUnitOfWorks)
                {
                    await dapper.CommitAsync();
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
        foreach (var dapper in _dapperUnitOfWorks)
        {
            await dapper.RollbackAsync();
        }
    }
}
