using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Logging.Services;
using System.Diagnostics;

namespace Synergy.Framework.Logging.Middleware;

internal class PerfLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _thresholdMs;

    public PerfLoggingMiddleware(RequestDelegate next, long thresholdMs)
    {
        _next = next;
        _thresholdMs = thresholdMs;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();

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

            // ILoggingService'i sadece bu isteğin scope'u içinde çözüyoruz
            var loggingService = httpContext.RequestServices.GetRequiredService<ILoggingService>();            

            if (executionTimeMs > _thresholdMs)
            {
                loggingService.LogPerf(executionTimeMs);
            }
        }
    }
}
