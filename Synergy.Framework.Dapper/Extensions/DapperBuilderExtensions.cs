using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Dapper.Configuration;
using Synergy.Framework.Dapper.Connections;
using Synergy.Framework.Dapper.Filters;
using Synergy.Framework.Dapper.Repositories;
using Synergy.Framework.Dapper.UnitOfWork;

namespace Synergy.Framework.Dapper.Extensions;

public static class DapperBuilderExtensions
{
    public static WebApplicationBuilder UseSynergyDapper(this WebApplicationBuilder builder, Action<DapperModuleOptions>? configure = null)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<TransactionFilter>();
        });

        var opt = new DapperModuleOptions();
        configure?.Invoke(opt);
        builder.Services.AddSingleton(opt);

        // DbConnection factory
        builder.Services.AddScoped<IDbConnectionFactory, DefaultDbConnectionFactory>();

        // Generic repositories & UoW
        builder.Services.AddScoped<IDbConnectionFactory, DefaultDbConnectionFactory>();
        builder.Services.AddScoped(typeof(IDapperReadRepository<,>), typeof(DapperReadRepository<,>));
        builder.Services.AddScoped(typeof(IDapperWriteRepository<,>), typeof(DapperWriteRepository<,>));
        builder.Services.AddScoped<IDapperUnitOfWork, DapperUnitOfWork>();

        // Audit handler
        //bu kullanıcı tarafında olacak
        //builder.Services.TryAddScoped<IAuditLogHandler, DefaultAuditLogHandler>();
        return builder;
    }
}

//builder.UseSynergyDapper(opt =>
//{
//    opt.SeparateReadDatabase      = true;
//    opt.ReadConnectionStringName  = "ReadDb";
//    opt.WriteConnectionStringName = "WriteDb";
//});