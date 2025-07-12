using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Auth.Configuration;
using Synergy.Framework.Auth.Data;
using Synergy.Framework.Auth.Entities;
using Synergy.Framework.Auth.Services;

namespace Synergy.Framework.Auth.Extensions;

public static class AuthBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyAuth(this WebApplicationBuilder builder, Action<AuthModuleOptions>? configure = null)
    {
        var opt = new AuthModuleOptions();
        configure?.Invoke(opt);
        builder.Services.AddSingleton(opt);
        builder.Services.AddHttpContextAccessor();

        // Identity DbContext ve Identity servislerini ekle
        builder.Services.AddDbContext<SynergyIdentityDbContext>(options =>
            options.UseSqlServer(opt.Identity?.ConnectionString));

        builder.Services.AddIdentity<SynergyIdentityUser, SynergyIdentityRole>()
            .AddEntityFrameworkStores<SynergyIdentityDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IAuthService, AuthService>();

        // Otomatik migration işlemi
        using (var scope = builder.Services.BuildServiceProvider().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SynergyIdentityDbContext>();
            dbContext.Database.Migrate();
        }

        return builder;
    }

    public static IApplicationBuilder UseSynergyAuthMiddlewares(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<AuthModuleOptions>();
        // Burada opsiyonel olarak middleware eklenebilir (ör: JWT, 2FA, Captcha, IP lock, vs.)
        return app;
    }
}
