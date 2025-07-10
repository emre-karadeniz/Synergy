using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Web.Common;
using Synergy.Framework.Web.Results;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Synergy.Framework.Web.Middlewares;

internal class RequestHandlingMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);

            if (httpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized) //401
            {
                await HandleUnauthorizedAsync(httpContext);
            }
            if (httpContext.Response.StatusCode == (int)HttpStatusCode.Forbidden) //403
            {
                await HandleForbiddenAsync(httpContext);
            }
        }
        catch (Exception)
        {
            throw; // Re-throw the exception
        }
    }

    private async Task HandleUnauthorizedAsync(HttpContext httpContext)
    {
        var result = Result.Unauthorized();
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        string response = JsonSerializer.Serialize(result, CachedJsonSerializerOptions);
        await httpContext.Response.WriteAsync(response);
    }

    private async Task HandleForbiddenAsync(HttpContext httpContext)
    {
        var result = Result.Forbidden();
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        string response = JsonSerializer.Serialize(result, CachedJsonSerializerOptions);
        await httpContext.Response.WriteAsync(response);
    } 
}

internal static class RequestHandlingMiddlewareExtensions
{
    internal static IApplicationBuilder UseRequestHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware<RequestHandlingMiddleware>();
}
