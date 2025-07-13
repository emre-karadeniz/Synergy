using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Novell.Directory.Ldap;
using Synergy.Framework.Auth.Configuration;
using Synergy.Framework.Auth.Data;
using Synergy.Framework.Auth.Entities;
using Synergy.Framework.Auth.Models;

namespace Synergy.Framework.Auth.Services;

internal class LoginService : ILoginService
{
    private readonly AuthModuleOptions _options;
    private readonly TokenService _tokenService;
    private readonly UserManager<SynergyIdentityUser> _userManager;
    private readonly SignInManager<SynergyIdentityUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly I2FaService? _twoFaService;
    private readonly ICaptchaService? _captchaService;
    private readonly IGoogleAuthService? _googleAuthService;

    public LoginService(AuthModuleOptions options, UserManager<SynergyIdentityUser> userManager, SignInManager<SynergyIdentityUser> signInManager, SynergyIdentityDbContext dbContext, IHttpContextAccessor httpContextAccessor, ICaptchaService? captchaService = null, I2FaService? twoFaService = null, IGoogleAuthService? googleAuthService = null)
    {
        _options = options;
        _tokenService = new TokenService(_options.TokenOptions, dbContext);
        _captchaService = captchaService;
        _userManager = userManager;
        _signInManager = signInManager;
        _twoFaService = twoFaService;
        _httpContextAccessor = httpContextAccessor;
        _googleAuthService = googleAuthService;
    }

    public async Task<AuthResult> IdentityLoginAsync(LoginRequest request)
    {
        var ipAddress = GetClientIpAddress(request.IPAddress);

        // Captcha kontrolü
        if (_options.EnableCaptcha && _captchaService != null)
        {
            var captchaValid = await _captchaService.ValidateCaptchaAsync(request.Captcha);
            if (!captchaValid)
                return new AuthResult { Success = false, ErrorMessage = "Captcha validation failed." };
        }

        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
        }

        // Email doğrulama kontrolü
        if (user.EmailConfirmed)
        {
            return new AuthResult { Success = false, ErrorMessage = "Email address is not confirmed." };
        }

        // 2FA kontrolü aktif ise sms gönder ve token üretme
        if (user.TwoFactorEnabled && _twoFaService != null)
        {
            if (string.IsNullOrEmpty(user.PhoneNumber))
                return new AuthResult { Success = false, ErrorMessage = "Phone number is required for 2FA." };

            await _twoFaService.SendSmsCodeAsync(user.PhoneNumber);
            return new AuthResult { Success = true, MaskedPhone = MaskPhone(user.PhoneNumber) };
        }

        var userId = user.Id;

        // Başarılı ise access ve refresh token üret
        var accessToken = await _tokenService.GenerateTokenAsync(user, ipAddress);

        return new AuthResult { Success = true, Token = accessToken.Token, RefreshToken = accessToken.RefreshToken };
    }

    public async Task<AuthResult> LdapLoginAsync(LdapLoginRequest request)
    {
        var ipAddress = GetClientIpAddress(request.IPAddress);

        if (_options.Ldap == null)
            return new AuthResult { Success = false, ErrorMessage = "LDAP ayarları eksik." };
        try
        {
            // Captcha kontrolü
            if (_options.EnableCaptcha && _captchaService != null)
            {
                var captchaValid = await _captchaService.ValidateCaptchaAsync(request.Captcha);
                if (!captchaValid)
                    return new AuthResult { Success = false, ErrorMessage = "Captcha validation failed." };
            }

            using var ldapConn = new LdapConnection();
            await ldapConn.ConnectAsync(_options.Ldap.Server, _options.Ldap.Port);
            await ldapConn.BindAsync(_options.Ldap.BindDn, _options.Ldap.BindPassword);

            // Kullanıcıyı bul ve doğrula
            var userDn = $"uid={request.UserName},{_options.Ldap.BaseDn}";
            await ldapConn.BindAsync(userDn, request.Password);

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
            }

            // Email doğrulama kontrolü
            if (user.EmailConfirmed)
            {
                return new AuthResult { Success = false, ErrorMessage = "Email address is not confirmed." };
            }

            // 2FA kontrolü aktif ise sms gönder ve token üretme
            if (user.TwoFactorEnabled && _twoFaService != null)
            {
                if (string.IsNullOrEmpty(user.PhoneNumber))
                    return new AuthResult { Success = false, ErrorMessage = "Phone number is required for 2FA." };

                await _twoFaService.SendSmsCodeAsync(user.PhoneNumber);
                return new AuthResult { Success = true, MaskedPhone = MaskPhone(user.PhoneNumber) };
            }

            var userId = request.UserName; // LDAP'da UserId olarak username kullanıyoruz

            // Başarılı ise access ve refresh token üret
            var accessToken = await _tokenService.GenerateTokenAsync(user, ipAddress);

            return new AuthResult { Success = true, Token = accessToken.Token, RefreshToken = accessToken.RefreshToken };
        }
        catch (LdapException ex)
        {
            return new AuthResult { Success = false, ErrorMessage = $"LDAP login failed: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = $"LDAP error: {ex.Message}" };
        }
    }

    public async Task<AuthResult> GoogleLoginAsync(string accessToken, string? ipAddress = null)
    {
        var ip = GetClientIpAddress(ipAddress);

        if (_googleAuthService == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "GoogleAuthService is not configured." };
        }

        var userInfo = await _googleAuthService.GetUserInfoAsync(accessToken);
        if (userInfo == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "Invalid Google access token." };
        }

        var user = await _userManager.FindByEmailAsync(userInfo.Email);
        if (user == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "User not found. Please register first." };
        }

        var userId = user.Id;
        var accessTokenJwt = await _tokenService.GenerateTokenAsync(user, ip);

        return new AuthResult { Success = true, Token = accessTokenJwt.Token, RefreshToken = accessTokenJwt.RefreshToken };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var ip = GetClientIpAddress(ipAddress);
        var tokenEntity = await _tokenService.GetRefreshTokenAsync(refreshToken);
        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            return new AuthResult { Success = false, ErrorMessage = "Refresh token is invalid or expired." };
        }

        if (_options.EnableIpLock && tokenEntity.CreatedByIp != ip)
        {
            return new AuthResult { Success = false, ErrorMessage = "IP address mismatch. Token usage denied." };
        }

        var user = await _userManager.FindByIdAsync(tokenEntity.Id.ToString());
        if (user == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "Kullanıcı bulunamadı." };
        }

        // Eski refresh token'ı iptal et
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        // Yeni access ve refresh token üret
        var newAccessToken = await _tokenService.GenerateTokenAsync(user, ip);

        return new AuthResult { Success = true, Token = newAccessToken.Token, RefreshToken = newAccessToken.RefreshToken };
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string userName, string code, string? ipAddress = null)
    {
        var ip = GetClientIpAddress(ipAddress);

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "Kullanıcı bulunamadı." };
        }

        if (!await _twoFaService.Validate2FaAsync(user.PhoneNumber, code))
        {
            return new AuthResult { Success = false, ErrorMessage = "Geçersiz 2FA kodu." };
        }

        // Başarılı ise access ve refresh token üret
        var accessToken = await _tokenService.GenerateTokenAsync(user, ip);

        return new AuthResult { Success = true, Token = accessToken.Token, RefreshToken = accessToken.RefreshToken };
    }

    private string GetClientIpAddress(string? ipAddress = null)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        // X-Forwarded-For kontrolü
        var forwardedIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedIp))
        {
            return forwardedIp.Split(',').First().Trim();
        }

        // RemoteIpAddress kontrolü
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
            return "*****";

        return new string('*', phone.Length - 4) + phone[^4..]; // örn: ******1234
    }
}
