using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
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
    public static WebApplicationBuilder AddSynergyApiDocumentation(this WebApplicationBuilder builder, Action<ApiDocumentationOptions>? configureOptions = null)
    {
        var docOptions = new ApiDocumentationOptions();
        configureOptions?.Invoke(docOptions);
        builder.Services.AddSingleton(docOptions);

        builder.Services.AddEndpointsApiExplorer(); // Hem Swagger hem Scalar için gerekli

        if (docOptions.EnableSwagger)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.ToString()); // Şema ID'lerini benzersiz hale getir

                c.SwaggerDoc(docOptions.Swagger.Version, new OpenApiInfo
                {
                    Title = docOptions.Swagger.Title,
                    Version = docOptions.Swagger.Version,
                    Description = docOptions.Swagger.Description
                });

                if (docOptions.Swagger.EnableBearerAuth)
                {
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
                }

                // XML yorumlarını dahil et (eğer etkinse)
                if (docOptions.Swagger.IncludeXmlComments)
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
            builder.Services.AddOpenApi(docOptions.Scalar.Title, options =>
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
        var docOptions = app.Services.GetService<IOptions<ApiDocumentationOptions>>()?.Value;
        if (docOptions == null)
        {
            return app; // Eğer seçenekler okunamazsa, hiçbir şey yapma.
        }

        if (app.Environment.IsDevelopment() || docOptions.EnableSwagger)
        {
            if (docOptions.EnableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/{docOptions.Swagger.RoutePrefix}/{docOptions.Swagger.Version}/swagger.json", $"{docOptions.Swagger.Title} {docOptions.Swagger.Version}");
                    c.RoutePrefix = docOptions.Swagger.RoutePrefix;
                    c.DocumentTitle = docOptions.Swagger.Title;
                });
            }
        }

        if (app.Environment.IsDevelopment() || docOptions.EnableScalar)
        {
            if (docOptions.EnableScalar)
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.Title = docOptions.Scalar.Title;
                    options.DarkMode = docOptions.Scalar.DarkMode;
                    options.Favicon = docOptions.Scalar.Favicon;
                    options.HideModels = docOptions.Scalar.HideModels;
                    options.ShowSidebar = docOptions.Scalar.ShowSidebar;

                    if (Enum.TryParse<ScalarLayout>(docOptions.Scalar.Layout, true, out var layout))
                    {
                        options.Layout = layout;
                    }
                    if (Enum.TryParse<ScalarTheme>(docOptions.Scalar.Theme, true, out var theme))
                    {
                        options.Theme = theme;
                    }

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
                var requirements = new Dictionary<string, OpenApiSecurityScheme>
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
