using Microsoft.AspNetCore.Builder;

namespace Synergy.Framework.ExternalService.Extensions;

public static class ExternalServiceBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyExternalService(this WebApplicationBuilder builder)
    {

        return builder;
    }

    public static IApplicationBuilder UseSynergyExternalServiceMiddlewares(this IApplicationBuilder app)
    {

        return app;
    }
}
