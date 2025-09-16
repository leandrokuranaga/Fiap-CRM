using EventStore.Client;
using Fiap.Contracts.Events;
using Fiap.CRM.Application.Auth.Services;
using Fiap.CRM.Application.Users.Services;
using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.Bus.Handlers;
using Fiap.CRM.Infra.CacheService.Services;
using Fiap.CRM.Infra.CrossCutting.Http.FCG;
using Fiap.CRM.Infra.Data;
using Fiap.CRM.Infra.Data.EventSourcing;
using Fiap.CRM.Infra.Data.Repositories;
using Fiap.CRM.Infra.HandleBus.Handlers;
using Fiap.CRM.Infra.HostedService;
using Fiap.CRM.Infra.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Refit;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Fiap.CRM.Infra.CrossCutting.IoC;

[ExcludeFromCodeCoverage]
public static class NativeInjector
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddLocalHttpClients(services, configuration);
        AddLocalServices(services, configuration);
        AddCustomAuthentication(services, configuration);
        AddRedisDatabase(services, configuration);
        AddObservability(services, configuration, services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>());
        AddDatabase(services, configuration);
        AddServiceBus(services, configuration);
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ConnectionStrings>()
            .Bind(configuration.GetSection(nameof(ConnectionStrings)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("CRMConnection");
        
        services.AddDbContext<Context>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

        services.AddSingleton(sp =>
        {
            var conn = configuration.GetConnectionString("EventStore");
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("EventStore connection string is not configured.");
            var settings = EventStoreClientSettings.Create(conn);
            return new EventStoreClient(settings);
        });
    }

    public static void AddLocalHttpClients(this IServiceCollection services, IConfiguration configuration) 
    {
        var urlFCG = configuration["ExternalServices:FCGUrl"];

        services
            .AddRefitClient<IFCGService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(urlFCG));
    }

    public static void AddLocalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<INotification, Notification>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();


        #region Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        #endregion

        #region Services
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IAuthService, AuthService>();
        #endregion

        #region Bus Services
        services.AutoRegisterHandlersFromAssemblyOf<AddGameToUserLibraryHandler>();
        services.AutoRegisterHandlersFromAssemblyOf<CreatedUserHandler>();
        services.AutoRegisterHandlersFromAssemblyOf<DeletedUserHandler>();
        services.AutoRegisterHandlersFromAssemblyOf<UpdatedUserHandler>();
        #endregion

    }

    public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(nameof(JwtSettings)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSection = configuration.GetSection(nameof(JwtSettings));
            var secretKey = jwtSection["SecretKey"];

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            };
        });

        services.AddAuthorization();
    }

    public static void AddRedisDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            options.ConfigurationOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions.ConnectTimeout = 10000;
            options.ConfigurationOptions.SyncTimeout = 10000;
            options.ConfigurationOptions.ConnectRetry = 5;
            options.ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
        });

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(60)
            };
        });
        
        services.AddSingleton<ICacheService, CacheService.Services.CacheService>();
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ConfigureSerilog(services, configuration, environment);
        ConfigureOpenTelemetry(services, configuration);
        return services;
    }

    public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceBus>()
            .Bind(configuration.GetSection(nameof(ServiceBus)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRebus(configure => configure
            .Transport(t => t.UseAmazonSQS(
                accessKeyId: configuration["ServiceBus:AccessKey"],
                secretAccessKey: configuration["ServiceBus:SecretKey"],
                regionEndpoint: Amazon.RegionEndpoint.GetBySystemName(configuration["ServiceBus:Region"]),
                inputQueueAddress: configuration["ServiceBus:CRMQueueName"]
            ))
            .Routing(r => r.TypeBased()
                .Map<AddGameToUserLibraryIntegrationEvent>(configuration["ServiceBus:CRMQueueName"])
                .Map<CreateUserIntegrationEvent>(configuration["ServiceBus:CRMQueueName"])
                .Map<UpdatedUserHandler>(configuration["ServiceBus:CRMQueueName"])
                .Map<DeletedUserHandler>(configuration["ServiceBus:CRMQueueName"])

                .Map<CreateUserIntegrationEvent>(configuration["ServiceBus:LambdaQueueName"]))
            .Options(o => 
            {
                o.RetryStrategy(maxDeliveryAttempts: 3);
                o.SetNumberOfWorkers(1);
                o.SetMaxParallelism(1);
            })
            );

        return services;
    }

    private static void ConfigureSerilog(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddOptions<Utils.Serilog>()
            .Bind(configuration.GetSection(nameof(Utils.Serilog)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var lokiUri = configuration["Serilog:WriteTo:1:Args:Uri"];

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", "CRM")
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
                uri: lokiUri,
                labels:
                [
                    new() { Key = "app", Value = "dotnet-api" },
                    new() { Key = "env", Value = environment.EnvironmentName }
                ])
            .CreateLogger();
    }

    private static void ConfigureOpenTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DistributedTracing>()
            .Bind(configuration.GetSection(nameof(DistributedTracing)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var serviceName = configuration["DistributedTracing:Jaeger:ServiceName"];
        var endpoint = configuration["DistributedTracing:Jaeger:Endpoint"];
        var host = configuration["DistributedTracing:Jaeger:Host"];
        var port = int.Parse(configuration["DistributedTracing:Jaeger:Port"] ?? "14268");

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddJaegerExporter(options =>
                {
                    if (!string.IsNullOrEmpty(endpoint))
                    {
                        options.Endpoint = new Uri(endpoint);
                        options.Protocol = JaegerExportProtocol.HttpBinaryThrift;
                    }
                    else
                    {
                        options.AgentHost = host;
                        options.AgentPort = port;
                    }
                }));
    }
}