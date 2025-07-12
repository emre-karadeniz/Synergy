namespace Synergy.Framework.Auth.Services;

public interface I2FaService
{
    Task<bool> Validate2FaAsync(string userId, string? code);
}

public interface ICaptchaService
{
    Task<bool> ValidateCaptchaAsync(string? captchaResponse);
}
