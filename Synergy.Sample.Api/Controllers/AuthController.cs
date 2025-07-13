using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synergy.Framework.Auth.Models;
using Synergy.Framework.Auth.Services;
using Synergy.Framework.Web.Base;
using Synergy.Framework.Web.Results;
using Synergy.Sample.Api.Models.Auth;

namespace Synergy.Sample.Api.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterManual([FromBody] RegisterManualRequest request)
    {
        var result = await _authService.RegisterManuallyAsync(new RegisterRequest
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        });
        return CreateActionResult(result.Success
            ? Result.Created("Kullanıcı kaydedildi.")
            : Result.BadRequest(result.ErrorMessage ?? "Kayıt başarısız."));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterGoogle([FromBody] RegisterGoogleRequest request)
    {
        var result = await _authService.RegisterWithGoogleAsync(request.AccessToken);
        return CreateActionResult(result.Success
            ? Result.Created("Google ile kullanıcı kaydedildi.")
            : Result.BadRequest(result.ErrorMessage ?? "Google ile kayıt başarısız."));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LoginManual([FromBody] LoginManualRequest request)
    {
        var result = await _authService.IdentityLoginAsync(new LoginRequest
        {
            UserName = request.UserName,
            Password = request.Password
        });
        return CreateActionResult(result.Success
            ? Result.Success("Login başarılı.")
            : Result.BadRequest(result.ErrorMessage ?? "Login başarısız."));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LoginGoogle([FromBody] LoginGoogleRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request.AccessToken);
        return CreateActionResult(result.Success
            ? Result.Success("Google ile login başarılı.")
            : Result.BadRequest(result.ErrorMessage ?? "Google ile login başarısız."));
    }
}
