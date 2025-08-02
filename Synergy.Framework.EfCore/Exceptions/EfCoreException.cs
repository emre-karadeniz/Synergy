namespace Synergy.Framework.EfCore.Exceptions;

internal class EfCoreException: Exception
{
    public string Code { get; set; }
    public EfCoreException(string message, string code = "GENERIC_ERROR", Exception? inner = null) : base(message, inner)
    {
        Code = code;
    }
}
