namespace Synergy.Framework.Auth.Configuration;

public class AuthModuleOptions
{
    public LdapOptions? Ldap { get; set; }
    public IdentityOptions? Identity { get; set; }
    public bool Enable2FA { get; set; } = false;
    public bool EnableCaptcha { get; set; } = false;
    public bool EnableEmailVerification { get; set; } = false;
    public bool EnableIpLock { get; set; } = false;
    public bool EnableRefreshToken { get; set; } = true;
    public bool EnableOAuth2 { get; set; } = false;
    public bool EnablePhoneLogin { get; set; } = false;
    public TokenOptions TokenOptions { get; set; } = null!;
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

public class IdentityOptions
{
    public string ConnectionStringName { get; set; } = "DefaultConnection";
}

public class TokenOptions
{
    public string Audience { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public int AccessTokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
    public string SigningKey { get; set; } = null!;
}