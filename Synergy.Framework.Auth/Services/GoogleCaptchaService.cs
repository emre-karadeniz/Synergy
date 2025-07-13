using Synergy.Framework.Auth.Configuration;
using System.Text.Json;

namespace Synergy.Framework.Auth.Services;

internal class GoogleCaptchaService : ICaptchaService
{
    private readonly AuthModuleOptions _options;
    private readonly HttpClient _httpClient;
    public GoogleCaptchaService(AuthModuleOptions options, HttpClient httpClient)
    {       
        _options = options;
        _httpClient = httpClient;
    }
    public async Task<bool> ValidateCaptchaAsync(string? captchaToken)
    {
        if (_options.EnableCaptcha)
            return true;

        var response = await _httpClient.PostAsync(
                $"{_options.CaptchaOptions.Url}?secret={_options.CaptchaOptions.SecretKey}&response={captchaToken}",
                null);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GoogleCaptchaResponse>(json);

        return result?.Success == true && result.Score >= 0.5;
    }

    private class GoogleCaptchaResponse
    {
        public bool Success { get; set; }
        public float Score { get; set; }
    }
}
