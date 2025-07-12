using Synergy.Framework.Auth.Models;

namespace Synergy.Framework.Auth.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> LdapLoginAsync(LdapLoginRequest request);
    Task<AuthResult> RegisterManuallyAsync(RegisterRequest request);
    Task<AuthResult> RegisterWithGoogleAsync(string accessToken);
    Task<AuthResult> RegisterWithPhoneAsync(string phoneNumber, string verificationCode);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress);
}
