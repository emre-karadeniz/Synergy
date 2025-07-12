namespace Synergy.Framework.Auth.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}

public class LoginRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Captcha { get; set; }
    public string? TwoFactorCode { get; set; }
}

public class LdapLoginRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Captcha { get; set; } 
    public string? TwoFactorCode { get; set; }
}

public class RegisterRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
