using Microsoft.AspNetCore.Identity;
using Synergy.Framework.Auth.Entities;
using Synergy.Framework.Auth.Models;

namespace Synergy.Framework.Auth.Services;

internal class RegisterService: IRegisterService
{
    private readonly UserManager<SynergyIdentityUser> _userManager;
    private readonly IGoogleAuthService? _googleAuthService;
    private readonly ISmsService? _smsService;
    public RegisterService(
        UserManager<SynergyIdentityUser> userManager,
        IGoogleAuthService? googleAuthService = null,
        ISmsService? smsService = null
    )
    {
        _userManager = userManager;
        _googleAuthService = googleAuthService;
        _smsService = smsService;
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
            return new AuthResult { Success = false, ErrorMessage = "Bu email ile sistemde kayıtlı bir kullanıcı zaten var." };
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
}
