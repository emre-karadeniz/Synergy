using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Logging.Services;

namespace Synergy.Framework.Logging.Middleware;

internal class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _excludeExceptionTypes;
    public ErrorLoggingMiddleware(RequestDelegate next, List<string> excludeExceptionTypes)
    {
        _next = next;
        _excludeExceptionTypes = excludeExceptionTypes;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            var exTypeName = ex.GetType().Name;
            if (!string.IsNullOrEmpty(exTypeName) && !_excludeExceptionTypes.Contains(exTypeName))
            {
                var loggingService = httpContext.RequestServices.GetRequiredService<ILoggingService>();
                loggingService.LogError(ex); 
            }

            //throw;
        }
    }
}