using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Synergy.Framework.Web.Configurations;
using System.Reflection;

namespace Synergy.Framework.Web.Extensions;

/// <summary>
/// API dokümantasyonunu (Swagger/Scalar) yapılandırmak için uzantı metotları.
/// </summary>
public static class ApiDocumentationBuilderExtensions
{
    /// <summary>
    /// API dokümantasyon servislerini (Swagger veya Scalar) yapılandırır.
    /// </summary>
    public static WebApplicationBuilder AddSynergyApiDocumentation(this WebApplicationBuilder builder, Action<SwaggerDocOptions>? configureOptions = null)
    {
        var docOptions = new SwaggerDocOptions();
        configureOptions?.Invoke(docOptions);
        builder.Services.AddSingleton(docOptions);

        builder.Services.AddEndpointsApiExplorer(); // Hem Swagger hem Scalar için gerekli

        if (docOptions.EnableSwagger)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.ToString()); // Şema ID'lerini benzersiz hale getir

                c.SwaggerDoc(docOptions.Version, new OpenApiInfo
                {
                    Title = docOptions.Title,
                    Version = docOptions.Version,
                    Description = docOptions.Description
                });

                // JWT Bearer güvenlik tanımını ekle
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                // Tüm endpoint'ler için güvenlik gereksinimini ekle (varsayılan olarak)
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // XML yorumlarını dahil et (eğer etkinse)
                if (docOptions.IncludeXmlComments)
                {
                    var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml"; // Uygulamanın ana assembly'si
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: XML Documentation file not found at '{xmlPath}'. API documentation might be incomplete.");
                    }
                }
            });
        }

        if (docOptions.EnableScalar)
        {
            // Scalar için OpenApi tanımını ekle
            builder.Services.AddOpenApi(docOptions.Version, options =>
            {
                options.AddDocumentTransformer<ScalarBearerSecuritySchemeTransformer>();
                // Ek Scalar OpenApi ayarları buraya gelebilir
            });
        }

        return builder;
    }

    /// <summary>
    /// HTTP request pipeline'ına API dokümantasyon UI'larını (Swagger UI veya Scalar UI) ekler.
    /// </summary>
    public static WebApplication UseSynergyApiDocumentation(this WebApplication app)
    {
        //var docOptions = app.Configuration
        //    .GetSection(SwaggerDocOptions.ConfigurationSectionName)
        //    .Get<SwaggerDocOptions>();
        var docOptions = app.Services.GetService<IOptions<SwaggerDocOptions>>()?.Value;
        //var docOptions = app.ApplicationServices.GetRequiredService<SwaggerDocOptions>();

        if (docOptions == null)
        {
            return app; // Eğer seçenekler okunamazsa, hiçbir şey yapma.
        }

        // Geliştirme ortamında veya her zaman etkinleştirilebilir
        if (app.Environment.IsDevelopment() || docOptions.EnableSwagger) // Production'da da Swagger'ı açmak istersek
        {
            if (docOptions.EnableSwagger)
            {
                app.UseSwagger(); // Swagger JSON endpoint'ini etkinleştirir
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{docOptions.Version}/swagger.json", $"{docOptions.Title} {docOptions.Version}");
                    c.RoutePrefix = docOptions.RoutePrefix;
                    c.DocumentTitle = docOptions.Title; // Tarayıcı başlığını ayarla
                });
            }
        }

        if (app.Environment.IsDevelopment() || docOptions.EnableScalar) // Production'da da Scalar'ı açmak istersek
        {
            if (docOptions.EnableScalar)
            {
                app.MapOpenApi(); // Scalar'ın OpenAPI JSON endpoint'ini etkinleştirir
                app.MapScalarApiReference(options =>
                {
                    options.Title = docOptions.Scalar.Title;
                    options.DarkMode = docOptions.Scalar.DarkMode;
                    options.Favicon = docOptions.Scalar.Favicon;
                    options.HideModels = docOptions.Scalar.HideModels;
                    options.ShowSidebar = docOptions.Scalar.ShowSidebar;

                    // String değerlerden enum'a dönüştürme
                    if (Enum.TryParse<ScalarLayout>(docOptions.Scalar.Layout, true, out var layout))
                    {
                        options.Layout = layout;
                    }
                    if (Enum.TryParse<ScalarTheme>(docOptions.Scalar.Theme, true, out var theme))
                    {
                        options.Theme = theme;
                    }

                    // HTTP Client seçimi
                    ScalarTarget target = ScalarTarget.CSharp;
                    if (Enum.TryParse<ScalarTarget>(docOptions.Scalar.DefaultHttpClientTarget, true, out var parsedTarget))
                    {
                        target = parsedTarget;
                    }
                    ScalarClient client = ScalarClient.RestSharp;
                    if (Enum.TryParse<ScalarClient>(docOptions.Scalar.DefaultHttpClientClient, true, out var parsedClient))
                    {
                        client = parsedClient;
                    }
                    options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(target, client);


                    options.Authentication = new ScalarAuthenticationOptions
                    {
                        PreferredSecuritySchemes = new[] { docOptions.Scalar.PreferredSecurityScheme }
                    };

                    // Scalar'ın kendi route prefix'i yok, OpenAPI endpoint'ini kullanır
                    // options.RoutePrefix = docOptions.RoutePrefix; // Bu satır Scalar için geçerli değil.
                });
            }
        }

        return app;
    }

    /// <summary>
    /// Scalar için Bearer güvenlik şemasını OpenApi dokümanına ekleyen transformer.
    /// </summary>
    internal sealed class ScalarBearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                // Add the security scheme at the document level
                var requirements = new Dictionary<string, OpenApiSecurityScheme> // NSwag'ın OpenApiSecurityScheme'ini kullan
                {
                    ["Bearer"] = new()
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    }
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = requirements;

                // Apply it as a requirement for all operations
                foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
                {
                    operation.Value.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                    });
                }
            }
        }
    }
}
