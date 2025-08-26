<<<<<<< HEAD
﻿using Fiap.Contracts.Events;
using Fiap.CRM.Application.Auth.Services;
=======
﻿using Fiap.CRM.Application.Auth.Services;
>>>>>>> 346fbd1 (Funcionando Consultas usando Token da Keycloack)
using Fiap.CRM.Application.Users.Services;
using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.CacheService.Services;
using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Models;
using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service;
using Fiap.CRM.Infra.Data;
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
<<<<<<< HEAD
=======
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
>>>>>>> 346fbd1 (Funcionando Consultas usando Token da Keycloack)
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using StackExchange.Redis;
<<<<<<< HEAD
using System.Diagnostics.CodeAnalysis;
=======
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
>>>>>>> 346fbd1 (Funcionando Consultas usando Token da Keycloack)
using System.Text;

namespace Fiap.CRM.Infra.CrossCutting.IoC;

[ExcludeFromCodeCoverage]
public static class NativeInjector
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        AddLocalHttpClients(services, configuration);
        AddLocalServices(services, configuration);
        AddCustomAuthentication(services, configuration, environment);
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
    }

    public static void AddLocalHttpClients(this IServiceCollection services, IConfiguration configuration) { }

    public static void AddLocalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<INotification, Notification>();

        services.AddHostedService<OutboxProcessorService>();

        #region Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        #endregion

        #region Services
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddOptions<KeycloakOptions>()
        .Bind(configuration.GetSection("Keycloak"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

        services.AddHttpClient<IKeycloakService, KeycloakService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            if (string.IsNullOrEmpty(options.AuthorityHost))
                throw new ArgumentNullException(nameof(options.AuthorityHost), "O AuthorityHost do Keycloak não pode ser nulo");

            client.BaseAddress = new Uri(options.AuthorityHost);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        #endregion

        #region Bus Services
        services.AutoRegisterHandlersFromAssemblyOf<AddGameToUserLibraryHandler>();
        #endregion

    }

    public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {

        // Vincula as configurações à classe KeycloakSettings
        var keycloakSettings = new KeycloakSettings();
        configuration.GetSection(KeycloakSettings.SectionName).Bind(keycloakSettings);
        services.AddSingleton(keycloakSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {           
            options.Authority = keycloakSettings.Authority;
            options.Audience = keycloakSettings.Audience;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = keycloakSettings.Audience,
                ValidateAudience = true
            };

            if (environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }

            
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    
                    if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                    {
                      
                        var realmAccessClaim = context.Principal.FindFirst("realm_access");
                        var resourceAccessClaim = context.Principal.FindFirst("resource_access");

                        
                        if (realmAccessClaim != null && realmAccessClaim.Value.Length > 0)
                        {
                            var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                            if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (!string.IsNullOrEmpty(roleName))
                                    {
                                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }
                        
                        if (resourceAccessClaim != null && resourceAccessClaim.Value.Length > 0)
                        {
                            var resourceAccess = System.Text.Json.JsonDocument.Parse(resourceAccessClaim.Value);
                           
                            if (resourceAccess.RootElement.TryGetProperty(keycloakSettings.Audience, out var clientRolesElement))
                            {
                                if (clientRolesElement.TryGetProperty("roles", out var roles))
                                {
                                    foreach (var role in roles.EnumerateArray())
                                    {
                                        var roleName = role.GetString();
                                        if (!string.IsNullOrEmpty(roleName))
                                        {
                                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        });
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