using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.EfCore.Configuration;
using Synergy.Framework.EfCore.Context;
using Synergy.Framework.EfCore.Filters;
using Synergy.Framework.EfCore.Interceptor;
using Synergy.Framework.EfCore.Repositories;
using Synergy.Framework.EfCore.UnitOfWork;

namespace Synergy.Framework.EfCore.Extensions;

public static class EfCoreBuilderExtensions
{
    /// <summary>
    /// Registers EF Core contexts, repositories and UoW with a single call.
    /// </summary>
    public static WebApplicationBuilder UseSynergyEfCore<TWriteDbContext, TUserKey>(
        this WebApplicationBuilder builder,
        Action<EfCoreModuleOptions>? configure = null)
        where TWriteDbContext : BaseDbContext
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<TransactionFilter>();
        });

        builder.Services.AddScoped<INolockContext, NolockContext>();
        builder.Services.AddScoped<WithNoLockInterceptor>();

        var cfg = new EfCoreModuleOptions();
        configure?.Invoke(cfg);

        // register options singleton so other services (Repos/UnitOfWork) can resolve
        builder.Services.AddSingleton(cfg);

        var csWrite = builder.Configuration.GetConnectionString(
            cfg.SeparateReadDatabase ? cfg.WriteConnectionStringName : cfg.ConnectionStringName);

        // 1️ WRITE context
        builder.Services.AddDbContext<TWriteDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<WithNoLockInterceptor>();
            options.UseSqlServer(csWrite)
                   .AddInterceptors(interceptor);
        });


        // 2️ READ context (optional)
        if (cfg.SeparateReadDatabase)
        {
            var csRead = builder.Configuration.GetConnectionString(cfg.ReadConnectionStringName);
            builder.Services.AddDbContext<BaseDbContext>((serviceProvider, opt) =>
            {
                var interceptor = serviceProvider.GetRequiredService<WithNoLockInterceptor>();
                opt.UseSqlServer(csRead)
                   .AddInterceptors(interceptor);
            },
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);
        }

        // 3️ generic repository + UoW registration
        builder.Services.AddScoped(typeof(IEfCoreReadRepository<,>), typeof(EfCoreReadRepository<,,>));
        builder.Services.AddScoped(typeof(IEfCoreWriteRepository<,>), typeof(EfCoreWriteRepository<,,>));
        builder.Services.AddScoped(typeof(IEfCoreUnitOfWork<>), typeof(EfCoreUnitOfWork<,>));

        // 4️ Audit handler registration is optional; user can swap with NullAuditLogHandler.
        //bu kısım kullanıcı tarafında olacak
        //builder.Services.TryAddScoped<IAuditLogHandler, DefaultAuditLogHandler>();

        // 5️ Automatic migration (optional)
        if (cfg.AutoMigrate)
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<TWriteDbContext>();
            ctx.Database.Migrate();
        }

        return builder;
    }
}


//builder.UseSynergyEfCore<MyAppDbContext, Guid>(opt =>
//{
//    opt.SeparateReadDatabase = true;
//    opt.ReadConnectionStringName  = "MyReadDb";
//    opt.WriteConnectionStringName = "MyWriteDb";
//    opt.AutoMigrate = false;        // İstersen kapat
//});

//var app = builder.Build();
// Artık IEfCoreReadRepository / IEfCoreWriteRepository / IEfCoreUnitOfWork<TDb> hazır