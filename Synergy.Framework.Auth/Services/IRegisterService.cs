using Synergy.Framework.Auth.Models;

namespace Synergy.Framework.Auth.Services;

public interface IRegisterService
{
    Task<AuthResult> RegisterManuallyAsync(RegisterRequest request);
    Task<AuthResult> RegisterWithGoogleAsync(string accessToken);
    Task<AuthResult> RegisterWithPhoneAsync(string phoneNumber, string verificationCode);
}
