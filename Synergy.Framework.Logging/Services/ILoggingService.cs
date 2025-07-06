namespace Synergy.Framework.Logging.Services;

public interface ILoggingService
{
    void LogAuth(string message, bool isSuccessful);
    void LogError(Exception exception);
    void LogPerf(long elapsedMilliseconds);
    void LogRequest(long executionTimeMs);
    void LogRateLimiting();
    void LogInformation(string message, string logType = "Info", object? payload = null); // Ek genel log metodu
}
