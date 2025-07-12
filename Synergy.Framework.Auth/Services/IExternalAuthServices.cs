using System.Text.Json.Serialization;

namespace Synergy.Framework.Auth.Services;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> GetUserInfoAsync(string accessToken);
}

public interface ISmsService
{
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
    Task<bool> ValidateVerificationCodeAsync(string phoneNumber, string code);
}

public class GoogleUserInfo
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
}
