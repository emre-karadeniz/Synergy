using Synergy.Framework.Auth.Extensions;
using Synergy.Framework.Logging.Extensions;
using Synergy.Framework.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.UseSynergyLogging(options =>
{
    options.ConnectionStringName = "SynergyLogDbConnection";
    //options.ExcludeExceptionTypes= new List<string> { "FormatException" };
});

builder.UseSynergyWeb();

builder.UseSynergyAuth(options =>
{
    options.Identity.ConnectionStringName = "SynergyCoreDbConnection";
    options.TokenOptions.Issuer = builder.Configuration["TokenOptions:Issuer"];
    options.TokenOptions.Audience = builder.Configuration["TokenOptions:Audience"];
    options.TokenOptions.SigningKey = builder.Configuration["TokenOptions:SigningKey"];
    options.TokenOptions.AccessTokenExpiration = int.Parse(builder.Configuration["TokenOptions:AccessTokenExpiration"]);
    options.TokenOptions.RefreshTokenExpiration = int.Parse(builder.Configuration["TokenOptions:RefreshTokenExpiration"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSynergyLoggingMiddlewares();
app.UseSynergyWebMiddlewares();

app.UseHttpsRedirection();

app.UseSynergyAuthMiddlewares();

app.MapControllers();

app.Run();
