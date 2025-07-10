using Microsoft.AspNetCore.Http;

namespace Synergy.Framework.Web.Extensions;

public static class HttpContextExtensions
{
    public static void SetValue(this HttpContext httpContext, string key, object value)
    {
        if (httpContext.Items.ContainsKey(key))
        {
            httpContext.Items[key] = value;
        }
        else
        {
            httpContext.Items.Add(key, value);
        }
    }

    public static T? GetValue<T>(this HttpContext httpContext, string key)
    {
        if (httpContext.Items.ContainsKey(key))
        {
            return (T?)httpContext.Items[key];
        }
        return default;
    }
}
