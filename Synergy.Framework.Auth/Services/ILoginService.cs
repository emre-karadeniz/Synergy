using Synergy.Framework.Auth.Models;

namespace Synergy.Framework.Auth.Services;

public interface ILoginService
{
    Task<AuthResult> IdentityLoginAsync(LoginRequest request);
    Task<AuthResult> LdapLoginAsync(LdapLoginRequest request);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress);
    Task<AuthResult> GoogleLoginAsync(string accessToken, string? ipAddress = null);
    Task<AuthResult> VerifyTwoFactorAsync(string userName, string code, string? ipAddress = null);
}
