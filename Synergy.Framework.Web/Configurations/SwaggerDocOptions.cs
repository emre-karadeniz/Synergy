namespace Synergy.Framework.Web.Configurations;

/// <summary>
/// API dokümantasyon ayarlarını temsil eder (Swagger ve Scalar için ayrı ayrı).
/// </summary>
public class ApiDocumentationOptions
{
    public const string ConfigurationSectionName = "ApiDocumentation";

    public bool EnableSwagger { get; set; } = true;
    public bool EnableScalar { get; set; } = false;
    public SwaggerOptions Swagger { get; set; } = new SwaggerOptions();
    public ScalarOptions Scalar { get; set; } = new ScalarOptions();
}

public class SwaggerOptions
{
    public string Title { get; set; } = "My API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "API Documentation";
    public string RoutePrefix { get; set; } = "swagger";
    public bool IncludeXmlComments { get; set; } = true;
    public bool EnableBearerAuth { get; set; } = true;
}

public class ScalarOptions
{
    public string Title { get; set; } = "My API Documentation";
    public string RoutePrefix { get; set; } = "scalar";
    public bool DarkMode { get; set; } = true;
    public string Favicon { get; set; } = "";
    public string DefaultHttpClientTarget { get; set; } = "CSharp";
    public string DefaultHttpClientClient { get; set; } = "RestSharp";
    public bool HideModels { get; set; } = false;
    public string Layout { get; set; } = "Modern";
    public string Theme { get; set; } = "BluePlanet";
    public bool ShowSidebar { get; set; } = true;
    public string PreferredSecurityScheme { get; set; } = "Bearer";
}