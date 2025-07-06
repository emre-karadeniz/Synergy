using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Logging.Services;
using System.Diagnostics;

namespace Synergy.Framework.Logging.Middleware;

internal class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _excludeRequestPaths;

    public RequestLoggingMiddleware(RequestDelegate next, List<string> excludeRequestPaths)
    {
        _next = next;
        _excludeRequestPaths = excludeRequestPaths ?? new List<string>();
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = httpContext.Request.Path.Value;

        try
        {
            await _next(httpContext);
        }
        catch (Exception)
        {
            throw; // Re-throw the exception
        }
        finally
        {
            stopwatch.Stop();
            var executionTimeMs = stopwatch.ElapsedMilliseconds;

            var endpoint = httpContext.GetEndpoint();
            if (endpoint is not null)
            {
                if (path != null && !_excludeRequestPaths.Contains(path))
                {
                    // ILoggingService'i sadece bu isteğin scope'u içinde çözüyoruz
                    var loggingService = httpContext.RequestServices.GetRequiredService<ILoggingService>();
                    loggingService.LogRequest(executionTimeMs);
                }
            }
        }
    }
}
