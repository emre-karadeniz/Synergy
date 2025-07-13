using Microsoft.Extensions.Caching.Distributed;

namespace Synergy.Framework.Auth.Services;

internal class TwoFactorService : I2FaService
{
    private readonly IDistributedCache _cache;

    public TwoFactorService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SendSmsCodeAsync(string phoneNumber)
    {
        //var code = GenerateNumericCode(4);
        var code = "9999";

        await _cache.SetStringAsync($"2fa:{phoneNumber}", code, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        // Burada SMS servisine entegre olursun, örn: Twilio veya NetGSM
        Console.WriteLine($"[SMS] Kod gönderildi: {code}"); // örnek
    }

    public async Task<bool> Validate2FaAsync(string phoneNumber, string? code)
    {
        var storedCode = await _cache.GetStringAsync($"2fa:{phoneNumber}");
        return storedCode == code;
    }

    private string GenerateNumericCode(int length)
    {
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => random.Next(0, 10).ToString()[0])
            .ToArray());
    }
}
