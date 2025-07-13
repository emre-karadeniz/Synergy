using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Synergy.Framework.Auth.Configuration;
using Synergy.Framework.Auth.Data;
using Synergy.Framework.Auth.Entities;
using Synergy.Framework.Auth.Services;
using Synergy.Framework.Shared.Exceptions;
using System.Text;

namespace Synergy.Framework.Auth.Extensions;

public static class AuthBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyAuth(this WebApplicationBuilder builder, Action<AuthModuleOptions>? configure = null)
    {
        var opt = new AuthModuleOptions();
        opt.Ldap = new LdapOptions();
        opt.Identity = new Configuration.IdentityOptions();
        opt.TokenOptions = new Configuration.TokenOptions();

        configure?.Invoke(opt);
        builder.Services.AddSingleton(opt);
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        var configuration = builder.Configuration;
        var connectionString = configuration.GetConnectionString(opt.Identity.ConnectionStringName)
                                ?? throw new SynergyException("Connection string not found.", "LOG_CONN_STRING_NULL");

        // Identity DbContext ve Identity servislerini ekle
        builder.Services.AddDbContext<SynergyIdentityDbContext>(options =>
            options.UseSqlServer(connectionString, x => x.MigrationsAssembly("Synergy.Framework.Auth")));

        builder.Services.AddIdentity<SynergyIdentityUser, SynergyIdentityRole>()
            .AddEntityFrameworkStores<SynergyIdentityDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // JWT Authentication ayarları
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = opt.TokenOptions.Issuer,
                ValidAudience = opt.TokenOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.TokenOptions.SigningKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

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

        // Authentication ve Authorization middleware'lerini ekle
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}


//önce dotnet-ef tool yükle
//dotnet tool install --global dotnet-ef
//buda güncelleme kodu lazım olursa diye
//dotnet tool update --global dotnet-ef

//kurduktan sonra vsyi yeniden başlat.
//Microsoft.EntityFrameworkCore.Design nuget paketinin kurulu olması lazım bu komutların çalışacağı katmanda
//Package Manager Console'da aşağıdaki komutları çalıştırabilirsin. Not: Default proje Synergy.Framework.Auth olmalı.

//dotnet ef migrations add InitialCreate --project Synergy.Framework.Auth --startup-project Synergy.Sample.Api
//dotnet ef database update --project Synergy.Framework.Auth --startup-project Synergy.Sample.Api