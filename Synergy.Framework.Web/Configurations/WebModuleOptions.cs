using Microsoft.AspNetCore.Mvc;

namespace Synergy.Framework.Web.Configurations;

public class WebModuleOptions
{
    /// <summary>
    /// API dokümantasyonunu (Swagger/Scalar) etkinleþtirir.
    /// </summary>
    public bool EnableApiDocumentation { get; set; } = true;
    /// <summary>
    /// API dokümantasyon ayarlarý (opsiyonel override için)
    /// </summary>
    public Action<SwaggerDocOptions>? ApiDocumentationOptions { get; set; }

    /// <summary>
    /// Rate Limiting'i etkinleþtirir.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
    /// <summary>
    /// Rate Limiting ayarlarý (opsiyonel override için)
    /// </summary>
    public Action<RateLimitingOptions>? RateLimitingOptions { get; set; }

    /// <summary>
    /// API davranýþýný (model validation, Synergy result) etkinleþtirir.
    /// </summary>
    public bool EnableApiBehavior { get; set; } = true;
    /// <summary>
    /// API davranýþ ayarlarý (opsiyonel override için)
    /// </summary>
    public Action<ApiBehaviorOptions>? ApiBehaviorOptions { get; set; }
}
