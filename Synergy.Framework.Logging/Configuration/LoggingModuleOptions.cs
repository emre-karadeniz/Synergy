using Synergy.Framework.Logging.Enums;

namespace Synergy.Framework.Logging.Configuration;

public class LoggingModuleOptions
{
    public bool EnableErrorLogging { get; set; } = true;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;
    public long PerformanceLogThresholdMs { get; set; } = 3000;

    public bool EnableLogging { get; set; } = true;
    public string[] UseLogDbType { get; set; } = [nameof(LogDbType.SqlServer)];
    public string ConnectionStringName { get; set; } = "DefaultConnection";
    public string TableName { get; set; } = "SynergySystemLogs";
    public bool AutoCreateSqlTable { get; set; } = true;
    public int BatchPostingLimit { get; set; } = 50;

    public List<string> ExcludeRequestPaths { get; set; } = new();
    public List<string> ExcludeExceptionTypes { get; set; } = new();
    public List<string> ExcludedRequestPathsForBody { get; set; } = new() { "/api/Auth/Login" };
}
