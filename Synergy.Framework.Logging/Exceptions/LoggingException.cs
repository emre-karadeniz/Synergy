namespace Synergy.Framework.Logging.Exceptions;

internal class LoggingException : Exception
{
    public string Code { get; set; }
    public LoggingException(string message, string code = "GENERIC_ERROR", Exception? inner = null) : base(message, inner)
    {
        Code = code;
    }
}
