using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Web.Configurations;
using Synergy.Framework.Web.Filters;
using Synergy.Framework.Web.Middlewares;
using Synergy.Framework.Web.Providers;
using System.Reflection;

namespace Synergy.Framework.Web.Extensions;

public static class WebBuilderExtensions
{
    /// <summary>
    /// Synergy web altyapısının tüm ana servislerini kolayca ekler.
    /// </summary>
    public static WebApplicationBuilder UseSynergyWeb(this WebApplicationBuilder builder, Action<WebModuleOptions>? configure = null)
    {
        var options = new WebModuleOptions();
        configure?.Invoke(options);

        builder.Services.AddControllers(controllerOptions =>
        {
            controllerOptions.Filters.Add<TrimStringsActionFilter>();
        });

        // API davranışı
        if (options.EnableApiBehavior)
        {
            builder.Services.Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
            {
                if (options.ApiBehaviorOptions != null)
                {
                    options.ApiBehaviorOptions(apiBehaviorOptions);
                }
                else
                {
                    ApiBehaviorConfiguration.ConfigureSynergyApiBehavior(apiBehaviorOptions);
                }
            });
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped(typeof(IUserIdentifierProvider<>), typeof(HttpUserIdentifierProvider<>));

        // API dokümantasyonu
        if (options.EnableApiDocumentation)
        {
            builder.AddSynergyApiDocumentation(options.ApiDocumentationOptions);
        }

        // Rate Limiting
        if (options.EnableRateLimiting)
        {
            builder.Services.AddSynergyRateLimiting(options.RateLimitingOptions);
        }

        return builder;
    }

    /// <summary>
    /// Synergy web altyapısının tüm ana middleware'lerini kolayca ekler.
    /// </summary>
    public static IApplicationBuilder UseSynergyWebMiddlewares(this IApplicationBuilder app)
    {
        app.UseRequestHandler();
        app.UseSynergyRateLimiting();
        if (app is WebApplication webApp)
        {
            webApp.UseSynergyApiDocumentation();
        }
        return app;
    }

    private static void LoadOtherImplementations(IServiceCollection services, string typeSuffix, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            return;

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(typeSuffix))
                .ToList();

            foreach (var implementation in types)
            {
                var interfaceType = implementation.GetInterface($"I{implementation.Name}");
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, implementation);
                }
            }
        }
    }

    public static void AddSynergyServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        LoadOtherImplementations(services, "Service", assemblies);
    }

    public static void AddSynergyRepositories(this IServiceCollection services, params Assembly[] assemblies)
    {
        LoadOtherImplementations(services, "Repository", assemblies);
    }
}


//builder.Services.AddSynergyServices(typeof(SomeService).Assembly, typeof(SomeOtherService).Assembly);
//builder.Services.AddSynergyRepositories(typeof(SomeRepository).Assembly);