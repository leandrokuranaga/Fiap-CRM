# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fiap.CRM.Api/Fiap.CRM.Api.csproj", "src/Fiap.CRM.Api/"]
COPY ["src/Fiap.CRM.Application/Fiap.CRM.Application.csproj", "src/Fiap.CRM.Application/"]
COPY ["src/Fiap.CRM.Domain/Fiap.CRM.Domain.csproj", "src/Fiap.CRM.Domain/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting.CacheService/Fiap.CRM.Infra.CrossCutting.CacheService.csproj", "src/Fiap.CRM.Infra.CrossCutting.CacheService/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting.Http/Fiap.CRM.Infra.CrossCutting.Http.csproj", "src/Fiap.CRM.Infra.CrossCutting.Http/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting.IoC/Fiap.CRM.Infra.CrossCutting.IoC.csproj", "src/Fiap.CRM.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.CRM.Infra.HostedService/Fiap.CRM.Infra.HostedService.csproj", "src/Fiap.CRM.Infra.HostedService/"]
COPY ["src/Fiap.CRM.Infra.Utils/Fiap.CRM.Infra.Utils.csproj", "src/Fiap.CRM.Infra.Utils/"]
COPY ["src/Fiap.CRM.Infra/Fiap.CRM.Infra.Data.csproj", "src/Fiap.CRM.Infra/"]
RUN dotnet restore "./src/Fiap.CRM.Api/Fiap.CRM.Api.csproj"
COPY . .
WORKDIR "/src/src/Fiap.CRM.Api"
RUN dotnet build "./Fiap.CRM.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.CRM.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.CRM.Api.dll"]