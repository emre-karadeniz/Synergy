namespace Synergy.Framework.Auth.Exceptions;

internal class AuthException : Exception
{
    public string Code { get; set; }
    public AuthException(string message, string code = "GENERIC_ERROR", Exception? inner = null) : base(message, inner)
    {
        Code = code;
    }
}
