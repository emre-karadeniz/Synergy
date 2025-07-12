using Microsoft.AspNetCore.Builder;

namespace Synergy.Framework.Core.Extensions;

public static class CoreBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyCore(this WebApplicationBuilder builder)
    {

        return builder;
    }

    public static IApplicationBuilder UseSynergyCoreMiddlewares(this IApplicationBuilder app)
    {

        return app;
    }
}
