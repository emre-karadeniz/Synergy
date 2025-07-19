namespace Synergy.Framework.Auth.Configuration;

public class AuthModuleOptions
{
    public LdapOptions? Ldap { get; set; }
    public IdentityOptionsEx? Identity { get; set; }
    public bool EnableCaptcha { get; set; } = false;
    public bool EnableIpLock { get; set; } = false;
    public bool EnableRefreshToken { get; set; } = true;
    public bool EnableOAuth2 { get; set; } = false;
    public bool EnablePhoneLogin { get; set; } = false;
    public TokenOptions TokenOptions { get; set; } = null!;
    public CaptchaOptions CaptchaOptions { get; set; } = null!;
}

public class LdapOptions
{
    public string Server { get; set; }
    public int Port { get; set; } = 389;
    public string BaseDn { get; set; }
    public string Domain { get; set; }
    public string BindDn { get; set; }
    public string BindPassword { get; set; }
}

public class IdentityOptionsEx
{
    public string ConnectionStringName { get; set; } = "DefaultConnection";
    public PasswordOptionsEx Password { get; set; } = new();
    public LockoutOptionsEx Lockout { get; set; } = new();
    public UserOptionsEx User { get; set; } = new();
    public SignInOptionsEx SignIn { get; set; } = new();
}

public class PasswordOptionsEx
{
    public int RequiredLength { get; set; } = 6;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public int RequiredUniqueChars { get; set; } = 1;
}

public class LockoutOptionsEx
{
    public bool AllowedForNewUsers { get; set; } = true;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int DefaultLockoutTimeSpanMinutes { get; set; } = 5;
}

public class UserOptionsEx
{
    public bool RequireUniqueEmail { get; set; } = true;
    public string AllowedUserNameCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
}

public class SignInOptionsEx
{
    public bool RequireConfirmedEmail { get; set; } = false;
    public bool RequireConfirmedPhoneNumber { get; set; } = false;
}

public class TokenOptions
{
    public string Audience { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public int AccessTokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
    public string SigningKey { get; set; } = null!;
}

public class CaptchaOptions
{
    public string SecretKey { get; set; } = "YOUR_GOOGLE_RECAPTCHA_SECRET";
    public string Url { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
}