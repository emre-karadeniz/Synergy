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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
