namespace Synergy.Framework.Web.Configurations;

/// <summary>
/// API dokümantasyon ayarlarını temsil eder (Swagger/Scalar için).
/// </summary>
public class SwaggerDocOptions
{
    public const string ConfigurationSectionName = "ApiDocumentation";

    public string Title { get; set; } = "My API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "API Documentation";
    public string RoutePrefix { get; set; } = "swagger"; // Hem Swagger hem de Scalar için varsayılan prefix
    public bool EnableSwagger { get; set; } = true; // Swagger'ı etkinleştir
    public bool EnableScalar { get; set; } = false; // Scalar'ı etkinleştir (Varsayılan olarak kapalı, çakışma olmasın diye)
    public bool IncludeXmlComments { get; set; } = true; // XML yorumlarını dahil et

    // Scalar'a özgü ayarlar
    public ScalarOptions Scalar { get; set; } = new ScalarOptions();
}

public class ScalarOptions
{
    public string Title { get; set; } = "My API Documentation";
    public bool DarkMode { get; set; } = true;
    public string Favicon { get; set; } = ""; // Kullanıcının favicon yolu
    public string DefaultHttpClientTarget { get; set; } = "CSharp"; // CSharp, Javascript, Python, Go
    public string DefaultHttpClientClient { get; set; } = "RestSharp"; // RestSharp, Fetch, Axios, Reqwest
    public bool HideModels { get; set; } = false;
    public string Layout { get; set; } = "Modern"; // Modern, Classic
    public string Theme { get; set; } = "BluePlanet"; // BluePlanet, Purple, Default
    public bool ShowSidebar { get; set; } = true;
    public string PreferredSecurityScheme { get; set; } = "Bearer";
}