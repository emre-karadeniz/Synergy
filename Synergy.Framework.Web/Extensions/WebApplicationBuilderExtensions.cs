using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Web.Configurations;
using Synergy.Framework.Web.Filters;
using Synergy.Framework.Web.Middlewares;
using Synergy.Framework.Web.Providers;

namespace Synergy.Framework.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureSynergyServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<TrimStringsActionFilter>();
        });

        // Burada API davranışını yapılandırıyoruz.
        // Varsayılan olarak Synergy Result kullanır.
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            ApiBehaviorConfiguration.ConfigureSynergyApiBehavior(options);
        });

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped(typeof(IUserIdentifierProvider<>), typeof(HttpUserIdentifierProvider<>));

        // Opsiyonel: Kullanıcı özel UserIdentifierProvider ekleyebilir. Api tarafında
        //builder.Services.AddScoped<IUserIdentifierProvider<Guid>, MyCustomUserProvider>();

        return builder;
    }

    public static IApplicationBuilder ConfigureSynergyPipeline(this IApplicationBuilder app)
    {
        app.UseRequestHandler();
        return app;
    }
}
