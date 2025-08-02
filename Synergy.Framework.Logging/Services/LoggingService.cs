using Serilog;
using Synergy.Framework.Logging.Enums;
using Synergy.Framework.Logging.Models;
using Synergy.Framework.Logging.Options;
using System.Text.Json;

namespace Synergy.Framework.Logging.Services;

internal class LoggingService : ILoggingService
{
    private readonly Serilog.ILogger _logger;
    private static readonly string LogFolder = Path.Combine(AppContext.BaseDirectory, "LogLibrary");
    private static readonly string LogFilePath = Path.Combine(LogFolder, "LogLibraryError.txt");

    public LoggingService()
    {
        _logger = Log.Logger; // Global Serilog.Log.Logger'ı kullanır
    }

    public void LogAuth(string message, bool isSuccessful)
    {
        _logger
            .ForContext("LogType", nameof(LogType.Auth))
            .Information("{Type} | {Message} | {PayloadJson}",
            nameof(LogType.Auth), message, JsonSerializer.Serialize(new AuthLogDto { IsSuccessful = isSuccessful }, LoggingJsonSerializerOptions.Cached));
    }

    public void LogError(Exception exception)
    {
        _logger
            .ForContext("LogType", nameof(LogType.Error))
            .Error("{Type} | {Message} | {@PayloadJson}",
            nameof(LogType.Error), "An error occurred during the process!", JsonSerializer.Serialize(new ErrorLogDto { ExceptionType = exception.GetType().Name, ExceptionMessage = exception.Message, ExceptionStackTrace = exception?.StackTrace ?? "", InnerExceptionMessage = exception?.InnerException?.Message ?? "" }, LoggingJsonSerializerOptions.Cached));
    }

    public void LogPerf(long elapsedMilliseconds)
    {
        _logger
            .ForContext("LogType", nameof(LogType.Perf))
            .Debug("{Type} | {Message} | {PayloadJson}",
            nameof(LogType.Perf), "Request took too long.", JsonSerializer.Serialize(new PerfLogDto { ElapsedMilliseconds = elapsedMilliseconds }, LoggingJsonSerializerOptions.Cached));
    }

    public void LogRequest(long executionTimeMs)
    {
        _logger
            .ForContext("LogType", nameof(LogType.Request))
            .Information("{Type} | {Message} | {@PayloadJson}",
            nameof(LogType.Request), "Request logged.", JsonSerializer.Serialize(new RequestLogDto { ExecutionTimeMs = executionTimeMs }, LoggingJsonSerializerOptions.Cached));
    }

    public void LogRateLimiting()
    {
        _logger
            .ForContext("LogType", nameof(LogType.RateLimiting))
            .Warning("{Type} | {Message} | {@PayloadJson}",
            nameof(LogType.RateLimiting), "Rate limit exceeded.", "{}");
    }

    public void LogInformation(string message, string logType = "Info", object? payload = null)
    {
        _logger
            .ForContext("LogType", logType)
            .Information("{Type} | {Message} | {@PayloadJson}",
            logType, message, payload != null ? JsonSerializer.Serialize(payload, LoggingJsonSerializerOptions.Cached) : "{}");
    }

    public void LogToFile(string message)
    {
        try
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";

            File.AppendAllText(LogFilePath, logLine);
        }
        catch
        {
            // Dosya loglama başarısız olursa sessizce yut.
        }
        
    }
}
