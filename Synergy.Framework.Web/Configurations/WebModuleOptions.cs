using Microsoft.AspNetCore.Mvc;

namespace Synergy.Framework.Web.Configurations;

public class WebModuleOptions
{
    /// <summary>
    /// API dokümantasyonunu (Swagger/Scalar) etkinleştirir.
    /// </summary>
    public bool EnableApiDocumentation { get; set; } = true;
    /// <summary>
    /// API dokümantasyon ayarları (opsiyonel override için)
    /// </summary>
    public Action<ApiDocumentationOptions>? ApiDocumentationOptions { get; set; }

    /// <summary>
    /// Rate Limiting'i etkinleştirir.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
    /// <summary>
    /// Rate Limiting ayarları (opsiyonel override için)
    /// </summary>
    public Action<RateLimitingOptions>? RateLimitingOptions { get; set; }

    /// <summary>
    /// API davranışını (model validation, Synergy result) etkinleştirir.
    /// </summary>
    public bool EnableApiBehavior { get; set; } = true;
    /// <summary>
    /// API davranış ayarları (opsiyonel override için)
    /// </summary>
    public Action<ApiBehaviorOptions>? ApiBehaviorOptions { get; set; }
}
