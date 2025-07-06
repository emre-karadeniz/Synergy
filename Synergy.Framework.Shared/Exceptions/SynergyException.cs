namespace Synergy.Framework.Shared.Exceptions;

public class SynergyException: Exception
{
    public string Code { get; }

    public SynergyException(string message, string code = "GENERIC_ERROR", Exception? inner = null)
        : base(message, inner)
    {
        Code = code;
    }
}


//throw new SynergyException("Logger initialization failed.", "LOGGING_INIT_FAIL");
//her kütüphanede try cache il kullanıcıyı bilgilendirici hatalar dönülebilir.
//ve bu hatalar seri log ile kütüphane özelinde loglanabilir.