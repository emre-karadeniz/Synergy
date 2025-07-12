namespace Synergy.Framework.Auth.Configuration;

public class TokenOptions
{
    public string Audience { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public int AccessTokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
    public string SigningKey { get; set; } = null!;
}
