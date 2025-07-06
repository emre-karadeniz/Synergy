using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;
using UAParser;

namespace Synergy.Framework.Logging.Enrichers;

internal class RequestContextEnricher: ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly Parser _uaParser = Parser.GetDefault();
    private readonly List<string> _excludedRequestPathsForBody;

    public RequestContextEnricher(IHttpContextAccessor httpContextAccessor, List<string> excludedRequestPathsForBody)
    {
        _httpContextAccessor = httpContextAccessor;
        _excludedRequestPathsForBody = excludedRequestPathsForBody;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("nameid")?.Value;
        var ip = context.Connection?.RemoteIpAddress?.ToString() ?? "N/A";
        var host = context.Request.Host.Value;
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path.Value ?? "";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var correlationId = context.TraceIdentifier;
        var level = logEvent.Level.ToString();
        var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";

        var requestBody = context.Items.TryGetValue("RequestBody", out var body) ? body?.ToString() ?? "N/A" : "N/A";
        if (_excludedRequestPathsForBody.Any(p => requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            requestBody = "{}"; // hassas veri içerdiği için loglama
        }

        string browser = "Unknown", os = "Unknown", deviceType = "Unknown", clientType = "Unknown";

        try
        {
            var uaInfo = _uaParser.Parse(userAgent);
            browser = $"{uaInfo.UA.Family} {uaInfo.UA.Major}".Trim();
            os = $"{uaInfo.OS.Family} {uaInfo.OS.Major}".Trim();

            var deviceFamily = uaInfo.Device.Family.ToLower();
            if (deviceFamily.Contains("spider") || deviceFamily.Contains("bot") || uaInfo.Device.IsSpider)
                deviceType = "Robot/Crawler";
            else if (deviceFamily.Contains("mobile") || deviceFamily.Contains("phone"))
                deviceType = "Mobile";
            else if (deviceFamily.Contains("tablet"))
                deviceType = "Tablet";
            else if (userAgent.ToLower().Contains("postman") || userAgent.ToLower().Contains("curl"))
                deviceType = "API Tool";
            else
                deviceType = "Desktop";

            clientType = userAgent.ToLower().Contains("postman") ? "Postman" :
                         userAgent.ToLower().Contains("curl") ? "Curl" :
                         "Browser";
        }
        catch { }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId ?? "N/A"));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Level", level));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IPAddress", ip));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HostName", host));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", requestMethod));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", requestPath));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestBody", requestBody));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserAgent", userAgent));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Browser", browser));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("OperatingSystem", os));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("DeviceType", deviceType));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientType", clientType));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName", applicationName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TimeStamp", logEvent.Timestamp.DateTime));
    }
}
