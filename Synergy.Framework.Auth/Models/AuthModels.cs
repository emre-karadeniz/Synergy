namespace Synergy.Framework.Auth.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? MaskedPhone { get; set; }
    public bool TwoFactorRequired => Token == null;
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}

public class LoginRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Captcha { get; set; }
    public string? IPAddress { get; set; }
}

public class LdapLoginRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Captcha { get; set; } 
    public string? IPAddress { get; set; }
}

public class RegisterRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class AccessToken
{
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiration { get; set; }
}
