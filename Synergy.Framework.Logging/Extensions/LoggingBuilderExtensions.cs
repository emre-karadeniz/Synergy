using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.MSSqlServer;
using Synergy.Framework.Logging.Configuration;
using Synergy.Framework.Logging.Enrichers;
using Synergy.Framework.Logging.Enums;
using Synergy.Framework.Logging.Exceptions;
using Synergy.Framework.Logging.Filters;
using Synergy.Framework.Logging.Middleware;
using Synergy.Framework.Logging.Services;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;

namespace Synergy.Framework.Logging.Extensions;

public static class LoggingBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyLogging(this WebApplicationBuilder builder, Action<LoggingModuleOptions>? configureOptions = null)
    {
        var options = new LoggingModuleOptions();

        // Opsiyonel konfigürasyon uygula
        configureOptions?.Invoke(options);

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<RequestBodyActionFilter>();
        });

        // DI kaydı
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<ILoggingService, LoggingService>();
        builder.Services.AddSingleton<ILogAuditService, LogAuditService>();
        builder.Services.AddHttpContextAccessor();

        var environmentName = builder.Environment.EnvironmentName;

        // Serilog config

        if (!options.EnableLogging)
        {
            Console.WriteLine("Synergy Serilog is not enabled. Logging to Console only.");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
            builder.Host.UseSerilog();
            return builder;
        }

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Filter.ByIncludingOnly(logEvent =>
                logEvent.Properties.TryGetValue("LogType", out var value) &&
                Enum.TryParse<LogType>(value.ToString().Trim('"'), out _));

        // Enrich ayarları
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        loggerConfig = loggerConfig
            .Enrich.With(new RequestContextEnricher(httpContextAccessor, options.ExcludedRequestPathsForBody))
            .Enrich.WithProperty("Environment", environmentName);


        // SQL Server Sink
        if (options.UseLogDbType.Contains(nameof(LogDbType.SqlServer)))
        {
            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString(options.ConnectionStringName)
                                    ?? throw new LoggingException("Connection string not found.", "LOG_CONN_STRING_NULL");
           
            var columnOptions = new ColumnOptions();

            // Serilog'un varsayılan sütunlarını kaldır
            columnOptions.Store.Remove(StandardColumn.Message);
            columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            columnOptions.Store.Remove(StandardColumn.Level);
            columnOptions.Store.Remove(StandardColumn.TimeStamp);
            columnOptions.Store.Remove(StandardColumn.Exception);
            columnOptions.Store.Remove(StandardColumn.Properties);

            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                    new SqlColumn { ColumnName = "UserId", DataType = SqlDbType.UniqueIdentifier, AllowNull = true },
                    new SqlColumn { ColumnName = "Type", DataType = SqlDbType.NVarChar, DataLength = 15, AllowNull = false },
                    new SqlColumn { ColumnName = "Level", DataType = SqlDbType.NVarChar, DataLength = 15, AllowNull = false },
                    new SqlColumn { ColumnName = "Message", DataType = SqlDbType.NVarChar, DataLength = 250, AllowNull = false },
                    new SqlColumn { ColumnName = "TimeStamp", DataType = SqlDbType.DateTime2, AllowNull = false },
                    new SqlColumn { ColumnName = "CorrelationId", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = false },
                    new SqlColumn { ColumnName = "ApplicationName", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = false },
                    new SqlColumn { ColumnName = "Environment", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = false },
                    new SqlColumn { ColumnName = "IPAddress", DataType = SqlDbType.NVarChar, DataLength = 45, AllowNull = false },
                    new SqlColumn { ColumnName = "HostName", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = false },
                    new SqlColumn { ColumnName = "UserAgent", DataType = SqlDbType.NVarChar, DataLength = 500, AllowNull = false },
                    new SqlColumn { ColumnName = "Browser", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = false },
                    new SqlColumn { ColumnName = "OperatingSystem", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = false },
                    new SqlColumn { ColumnName = "DeviceType", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = false },
                    new SqlColumn { ColumnName = "ClientType", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = false },
                    new SqlColumn { ColumnName = "RequestMethod", DataType = SqlDbType.NVarChar, DataLength = 10, AllowNull = false },
                    new SqlColumn { ColumnName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 250, AllowNull = false },
                    new SqlColumn { ColumnName = "RequestBody", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = false },
                    new SqlColumn { ColumnName = "PayloadJson", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = false },
                    new SqlColumn { ColumnName = "IsAuthenticated", DataType = SqlDbType.Bit, AllowNull = false },
            };

            loggerConfig = loggerConfig.WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = options.TableName,
                    AutoCreateSqlTable = options.AutoCreateSqlTable,
                    BatchPostingLimit = options.BatchPostingLimit
                },
                columnOptions: columnOptions
            );

            // ElasticSearch Sink
            if (options.UseLogDbType.Contains(nameof(LogDbType.Elasticsearch)))
            {
                string elasticUri = options.ElasticsearchUri;
                loggerConfig = loggerConfig.WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        AutoRegisterTemplate = true, // Şablonu otomatik kaydet, false olursa kendine şablon oluşturmanız gerekir
                        IndexFormat = "synergy-logs-{0:yyyy.MM.dd}"
                    });
            }

            // Serilog'un kendi iç hatalarını konsola yazdırın (geliştirme için faydalı)
            SelfLog.Enable(msg => Console.WriteLine($"Serilog SelfLog: {msg}"));
            SelfLog.Enable(msg => Debug.WriteLine(msg));
            SelfLog.Enable(Console.Error);

            Log.Logger = loggerConfig.CreateLogger();
            builder.Host.UseSerilog();
        }

        return builder;
    }

    public static IApplicationBuilder UseSynergyLoggingMiddlewares(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<LoggingModuleOptions>();

        if (options.EnableErrorLogging)
        {
            app.UseMiddleware<ErrorLoggingMiddleware>(options.ExcludeExceptionTypes);
        }

        if (options.EnablePerformanceLogging)
        {
            app.UseMiddleware<PerfLoggingMiddleware>(options.PerformanceLogThresholdMs);
        }

        if (options.EnableRequestLogging)
        {
            app.UseMiddleware<RequestLoggingMiddleware>(options.ExcludeRequestPaths);
        }

        return app;
    }
}



//var builder = WebApplication.CreateBuilder(args);

//builder.UseSynergyLogging(options =>
//{
//    options.EnableErrorLogging = true;
//    options.EnableRequestLogging = true;
//    options.EnablePerformanceLogging = true;
//    options.PerformanceLogThresholdMs = 750;

//    options.EnableSqlLogging = true;
//    options.SqlConnectionStringName = "DefaultConnection";
//    options.SqlTableName = "SynergySystemLogs";
//    options.AutoCreateSqlTable = true;
//});

//var app = builder.Build();

//app.UseSynergyLoggingMiddlewares();

//app.Run();
