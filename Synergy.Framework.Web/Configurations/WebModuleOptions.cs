using Microsoft.AspNetCore.Mvc;

namespace Synergy.Framework.Web.Configurations;

public class WebModuleOptions
{
    /// <summary>
    /// API dok�mantasyonunu (Swagger/Scalar) etkinle�tirir.
    /// </summary>
    public bool EnableApiDocumentation { get; set; } = true;
    /// <summary>
    /// API dok�mantasyon ayarlar� (opsiyonel override i�in)
    /// </summary>
    public Action<SwaggerDocOptions>? ApiDocumentationOptions { get; set; }

    /// <summary>
    /// Rate Limiting'i etkinle�tirir.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
    /// <summary>
    /// Rate Limiting ayarlar� (opsiyonel override i�in)
    /// </summary>
    public Action<RateLimitingOptions>? RateLimitingOptions { get; set; }

    /// <summary>
    /// API davran���n� (model validation, Synergy result) etkinle�tirir.
    /// </summary>
    public bool EnableApiBehavior { get; set; } = true;
    /// <summary>
    /// API davran�� ayarlar� (opsiyonel override i�in)
    /// </summary>
    public Action<ApiBehaviorOptions>? ApiBehaviorOptions { get; set; }
}
