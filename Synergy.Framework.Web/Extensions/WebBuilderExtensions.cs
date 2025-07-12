using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Web.Configurations;
using Synergy.Framework.Web.Filters;
using Synergy.Framework.Web.Middlewares;
using Synergy.Framework.Web.Providers;

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
                    // Kullanıcıdan gelen Action<ApiBehaviorOptions> doğrudan uygulanır
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
        // WebApplication'a cast ederek UseSynergyApiDocumentation'u çağır
        if (app is WebApplication webApp)
        {
            webApp.UseSynergyApiDocumentation();
        }
        return app;
    }
}
