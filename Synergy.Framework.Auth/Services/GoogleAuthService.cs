using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Synergy.Framework.Auth.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;

    public GoogleAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve user info from Google.");
        }

        var content = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content);

        if (userInfo == null)
        {
            throw new Exception("Failed to parse user info from Google.");
        }

        return userInfo;
    }
}
