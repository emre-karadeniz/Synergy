using Microsoft.AspNetCore.Builder;

namespace Synergy.Framework.Auth.Extensions;

public static class AuthBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyAuth(this WebApplicationBuilder builder)
    {
        
        return builder;
    }

    public static IApplicationBuilder UseSynergyAuthMiddlewares(this IApplicationBuilder app)
    {

        return app;
    }
}
