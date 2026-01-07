using Fiap.CRM.Api.Extensions;
using Fiap.CRM.Api.Middlewares;
using Fiap.CRM.Infra.CrossCutting.IoC;
using Fiap.CRM.Infra.Utils;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.Extensions.Options;
using Prometheus;
using Serilog;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

#if DEBUG
builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
#endif

builder.Configuration.AddSystemsManager(source =>
{
    source.Path = "/myapp/prod/";
    source.Optional = true;
    source.ReloadAfter = TimeSpan.FromMinutes(5);
});

builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection(nameof(ConnectionStrings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var serviceProvider = builder.Services.BuildServiceProvider();
var databaseConnection = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value;

builder.Services.AddCustomServices(builder.Configuration);

builder.Host.UseSerilog();

builder.Services.AddCustomMvc();

builder.Services
    .AddHealthChecks()
    .AddApplicationStatus()
    .AddRedis(databaseConnection.Redis, name: "redis");

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGlobalCorsPolicy();

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseRouting();
app.UseSwaggerDocumentation();
app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.MapHealthChecks("/health");

app.UseHttpMetrics();
app.MapMetrics();

app.Run();


[ExcludeFromCodeCoverage]
public partial class Program { }