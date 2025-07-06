using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Logging.Services;
using Synergy.Framework.Shared.Options;
using System.Net;
using System.Text.Json;

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
                // ILoggingService'i sadece bu isteğin scope'u içinde çözüyoruz
                var loggingService = httpContext.RequestServices.GetRequiredService<ILoggingService>();
                loggingService.LogError(ex); 
            }
            //burada exculude olan için loglama yapılmadı ama geri dönüş 500 ayarlandı bunu bi incele

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            string response = JsonSerializer.Serialize(FailureResult.Failure(), SynergyJsonSerializerOptions.Cached);
            await httpContext.Response.WriteAsync(response);
        }
    }
}

internal record FailureResult(List<string> Messages, int StatusCode, bool IsSuccess = false)
{
    public static FailureResult Failure() => new(["An error occurred during the process!"], (int)HttpStatusCode.InternalServerError);
}