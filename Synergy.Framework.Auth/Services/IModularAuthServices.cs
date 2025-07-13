namespace Synergy.Framework.Auth.Services;

public interface I2FaService
{
    Task SendSmsCodeAsync(string phoneNumber);
    Task<bool> Validate2FaAsync(string phoneNumber, string? code);
}

public interface ICaptchaService
{
    Task<bool> ValidateCaptchaAsync(string? captchaToken);
}
