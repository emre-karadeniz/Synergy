namespace Synergy.Framework.Web.Configurations;

/// <summary>
/// Rate Limiting ayarlarını temsil eder. appsettings.json'dan okunur.
/// </summary>
public class RateLimitingOptions
{
    public const string ConfigurationSectionName = "RateLimiting";

    /// <summary>
    /// Global Rate Limiting'in etkinleştirilip etkinleştirilmeyeceği.
    /// </summary>
    public bool EnableGlobalLimiting { get; set; } = false;

    /// <summary>
    /// Global Rate Limiting politikası. Eğer belirtilmezse, FixedWindow kullanılır.
    /// </summary>
    public string GlobalPolicyName { get; set; } = "GlobalFixedWindowPolicy";

    /// <summary>
    /// Global Fixed Window Rate Limiter için ayarlar.
    /// </summary>
    public FixedWindowLimiterOptions GlobalFixedWindowOptions { get; set; } = new FixedWindowLimiterOptions();

    /// <summary>
    /// Global Sliding Window Rate Limiter için ayarlar.
    /// </summary>
    public SlidingWindowLimiterOptions GlobalSlidingWindowOptions { get; set; } = new SlidingWindowLimiterOptions();

    /// <summary>
    /// Global Token Bucket Rate Limiter için ayarlar.
    /// </summary>
    public TokenBucketLimiterOptions GlobalTokenBucketOptions { get; set; } = new TokenBucketLimiterOptions();

    /// <summary>
    /// Global Concurrency Limiter için ayarlar.
    /// </summary>
    public ConcurrencyLimiterOptions GlobalConcurrencyOptions { get; set; } = new ConcurrencyLimiterOptions();

    /// <summary>
    /// İstek reddedildiğinde dönülecek HTTP durum kodu. (Varsayılan: 429 Too Many Requests)
    /// </summary>
    public int RejectionStatusCode { get; set; } = 429;

    /// <summary>
    /// Kullanıcının özel olarak tanımlayabileceği endpoint bazlı veya kimlik bazlı politikalar.
    /// </summary>
    public Dictionary<string, EndpointRateLimiterOptions> EndpointPolicies { get; set; } = new Dictionary<string, EndpointRateLimiterOptions>();
}

/// <summary>
/// Sabit Pencere (Fixed Window) Rate Limiter ayarları.
/// </summary>
public class FixedWindowLimiterOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 1;
    public int QueueLimit { get; set; } = 0; // Varsayılan olarak sıraya almama
    public string QueueProcessingOrder { get; set; } = "OldestFirst"; // OldestFirst, NewestFirst
}

/// <summary>
/// Kayar Pencere (Sliding Window) Rate Limiter ayarları.
/// </summary>
public class SlidingWindowLimiterOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 1;
    public int SegmentsPerWindow { get; set; } = 8;
    public int QueueLimit { get; set; } = 0;
    public string QueueProcessingOrder { get; set; } = "OldestFirst";
}

/// <summary>
/// Token Kovası (Token Bucket) Rate Limiter ayarları.
/// </summary>
public class TokenBucketLimiterOptions
{
    public int TokenLimit { get; set; } = 10;
    public int QueueLimit { get; set; } = 0;
    public string QueueProcessingOrder { get; set; } = "OldestFirst";
    public int ReplenishmentPeriodSeconds { get; set; } = 1; // Kaç saniyede bir token ekleneceği
    public int TokensPerPeriod { get; set; } = 1; // Her periyotta kaç token ekleneceği
    public bool AutoReplenishment { get; set; } = true; // Otomatik token yenileme
}

/// <summary>
/// Eşzamanlılık (Concurrency) Rate Limiter ayarları.
/// </summary>
public class ConcurrencyLimiterOptions
{
    public int PermitLimit { get; set; } = 10;
    public int QueueLimit { get; set; } = 0;
    public string QueueProcessingOrder { get; set; } = "OldestFirst";
}

/// <summary>
/// Belirli endpoint veya kullanıcı bazlı politikalar için ayarlar.
/// </summary>
public class EndpointRateLimiterOptions
{
    public string PolicyName { get; set; } = "CustomPolicy";
    public string LimiterType { get; set; } = "FixedWindow"; // FixedWindow, SlidingWindow, TokenBucket, Concurrency

    public FixedWindowLimiterOptions FixedWindowOptions { get; set; } = new FixedWindowLimiterOptions();
    public SlidingWindowLimiterOptions SlidingWindowOptions { get; set; } = new SlidingWindowLimiterOptions();
    public TokenBucketLimiterOptions TokenBucketOptions { get; set; } = new TokenBucketLimiterOptions();
    public ConcurrencyLimiterOptions ConcurrencyOptions { get; set; } = new ConcurrencyLimiterOptions();
}