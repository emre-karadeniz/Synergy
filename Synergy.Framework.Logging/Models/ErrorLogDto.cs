namespace Synergy.Framework.Logging.Models;

internal class ErrorLogDto
{
    public string ExceptionType { get; set; } = null!;
    public string ExceptionMessage { get; set; } = null!;
    public string ExceptionStackTrace { get; set; } = null!;
    public string? InnerExceptionMessage { get; set; }
}
