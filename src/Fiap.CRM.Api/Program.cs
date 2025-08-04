using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Api.Extensions;
using Fiap.CRM.Api.Middlewares;
using Fiap.CRM.Infra.Utils;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

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
    .AddNpgSql(databaseConnection.DefaultConnection)
    .AddApplicationStatus()
    .AddRedis(databaseConnection.RedisConnectionString);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGlobalCorsPolicy();

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwaggerDocumentation();

app.UseRouting();

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.UseHttpMetrics();
app.MapMetrics();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }