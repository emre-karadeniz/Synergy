using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Novell.Directory.Ldap;
using Synergy.Framework.Auth.Configuration;
using Synergy.Framework.Auth.Data;
using Synergy.Framework.Auth.Entities;
using Synergy.Framework.Auth.Models;
using System.Security.Claims;

namespace Synergy.Framework.Auth.Services;

public class AuthService : IAuthService
{
    private readonly AuthModuleOptions _options;
    private readonly TokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<SynergyIdentityUser> _userManager;
    private readonly SignInManager<SynergyIdentityUser> _signInManager;
    private readonly I2FaService? _twoFaService;
    private readonly ICaptchaService? _captchaService;
    private readonly IGoogleAuthService? _googleAuthService;
    private readonly ISmsService? _smsService;

    public AuthService(
        AuthModuleOptions options,
        IHttpContextAccessor httpContextAccessor,
        UserManager<SynergyIdentityUser> userManager,
        SignInManager<SynergyIdentityUser> signInManager,
        SynergyIdentityDbContext dbContext,
        I2FaService? twoFaService = null,
        ICaptchaService? captchaService = null,
        IGoogleAuthService? googleAuthService = null,
        ISmsService? smsService = null
    )
    {
        _options = options;
        _tokenService = new TokenService(_options.TokenOptions, dbContext);
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _twoFaService = twoFaService;
        _captchaService = captchaService;
        _googleAuthService = googleAuthService;
        _smsService = smsService;
    }

    public async Task<AuthResult> IdentityLoginAsync(LoginRequest request)
    {
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

        // Email doðrulama kontrolü
        if (_options.EnableEmailVerification && !user.EmailConfirmed)
        {
            return new AuthResult { Success = false, ErrorMessage = "Email address is not confirmed." };
        }

        // 2FA kontrolü
        if (_options.Enable2FA && _twoFaService != null)
        {
            var twoFaValid = await _twoFaService.Validate2FaAsync(user.Id, request.TwoFactorCode);
            if (!twoFaValid)
                return new AuthResult { Success = false, ErrorMessage = "2FA validation failed." };
        }

        // IP'yi context'ten al
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = user.Id;

        // Baþarýlý ise access ve refresh token üret
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.NameIdentifier, userId) };
        var accessToken = _tokenService.GenerateToken(userId, claims);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, ip);
        await _tokenService.SaveUserSessionAsync(userId, accessToken, refreshToken.Token, ip);

        return new AuthResult { Success = true, Token = accessToken, RefreshToken = refreshToken.Token };
    }

    public async Task<AuthResult> LdapLoginAsync(LdapLoginRequest request)
    {
        if (_options.Ldap == null)
            return new AuthResult { Success = false, ErrorMessage = "LDAP ayarlarý eksik." };
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

            // Kullanýcýyý bul ve doðrula
            var userDn = $"uid={request.UserName},{_options.Ldap.BaseDn}";
            await ldapConn.BindAsync(userDn, request.Password);

            // 2FA kontrolü
            if (_options.Enable2FA && _twoFaService != null)
            {
                var twoFaValid = await _twoFaService.Validate2FaAsync(request.UserName, request.TwoFactorCode);
                if (!twoFaValid)
                    return new AuthResult { Success = false, ErrorMessage = "2FA validation failed." };
            }

            // IP'yi context'ten al
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = request.UserName; // LDAP'da UserId olarak username kullanýyoruz

            // Baþarýlý ise access ve refresh token üret
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, request.UserName), new Claim(ClaimTypes.NameIdentifier, userId) };
            var accessToken = _tokenService.GenerateToken(userId, claims);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, ip);
            await _tokenService.SaveUserSessionAsync(userId, accessToken, refreshToken.Token, ip);

            return new AuthResult { Success = true, Token = accessToken, RefreshToken = refreshToken.Token };
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

    public async Task<AuthResult> RegisterManuallyAsync(RegisterRequest request)
    {
        var user = new SynergyIdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResult { Success = false, ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)) };
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> RegisterWithGoogleAsync(string accessToken)
    {
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
        if (user != null)
        {
            return new AuthResult { Success = false, ErrorMessage = "Bu email ile sistemde kayýtlý bir kullanýcý zaten var." };
        }

        var newUser = new SynergyIdentityUser
        {
            UserName = userInfo.Email,
            Email = userInfo.Email
        };

        var result = await _userManager.CreateAsync(newUser);
        if (!result.Succeeded)
        {
            return new AuthResult { Success = false, ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)) };
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> RegisterWithPhoneAsync(string phoneNumber, string verificationCode)
    {
        if (_smsService == null)
        {
            return new AuthResult { Success = false, ErrorMessage = "SmsService is not configured." };
        }

        var isValid = await _smsService.ValidateVerificationCodeAsync(phoneNumber, verificationCode);
        if (!isValid)
        {
            return new AuthResult { Success = false, ErrorMessage = "Invalid verification code." };
        }

        var user = new SynergyIdentityUser
        {
            UserName = phoneNumber,
            PhoneNumber = phoneNumber
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return new AuthResult { Success = false, ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)) };
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        // IP'yi context'ten al
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var tokenEntity = await _tokenService.GetRefreshTokenAsync(refreshToken);
        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            return new AuthResult { Success = false, ErrorMessage = "Refresh token is invalid or expired." };
        }
        var session = await _tokenService.GetUserSessionByRefreshTokenAsync(refreshToken);
        if (session == null || session.IpAddress != ip)
        {
            return new AuthResult { Success = false, ErrorMessage = "IP address mismatch. Token usage denied." };
        }
        // Eski refresh token'ý iptal et
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        // Yeni access ve refresh token üret
        var userId = tokenEntity.UserId;
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var newAccessToken = _tokenService.GenerateToken(userId, claims);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, ip);
        await _tokenService.SaveUserSessionAsync(userId, newAccessToken, newRefreshToken.Token, ip);
        return new AuthResult { Success = true, Token = newAccessToken, RefreshToken = newRefreshToken.Token };
    }

    public async Task<AuthResult> GoogleLoginAsync(string accessToken)
    {
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

        // IP'yi context'ten al
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = user.Id;
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.NameIdentifier, userId) };
        var accessTokenJwt = _tokenService.GenerateToken(userId, claims);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, ip);
        await _tokenService.SaveUserSessionAsync(userId, accessTokenJwt, refreshToken.Token, ip);

        return new AuthResult { Success = true, Token = accessTokenJwt, RefreshToken = refreshToken.Token };
    }
}
