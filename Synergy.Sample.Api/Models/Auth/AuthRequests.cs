namespace Synergy.Sample.Api.Models.Auth;

public class RegisterManualRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class RegisterGoogleRequest
{
    public string AccessToken { get; set; }
}

public class LoginManualRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class LoginGoogleRequest
{
    public string AccessToken { get; set; }
}
