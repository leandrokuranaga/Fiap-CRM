FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fiap.CRM.Api/Fiap.CRM.Api.csproj", "src/Fiap.CRM.Api/"]
COPY ["src/Fiap.CRM.Application/Fiap.CRM.Application.csproj", "src/Fiap.CRM.Application/"]
COPY ["src/Fiap.CRM.Domain/Fiap.CRM.Domain.csproj", "src/Fiap.CRM.Domain/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting/Fiap.CRM.Infra.CrossCutting.csproj", "src/Fiap.CRM.Infra.CrossCutting/"]
COPY ["src/Fiap.CRM.Infra.HostedService/Fiap.CRM.Infra.HostedService.csproj", "src/Fiap.CRM.Infra.HostedService/"]
COPY ["src/Fiap.Contracts.Events/Fiap.Contracts.Events.csproj", "src/Fiap.Contracts.Events/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting.IoC/Fiap.CRM.Infra.CrossCutting.IoC.csproj", "src/Fiap.CRM.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.CRM.Infra.HandleBus/Fiap.CRM.Infra.Bus.csproj", "src/Fiap.CRM.Infra.HandleBus/"]
COPY ["src/Fiap.CRM.Infra/Fiap.CRM.Infra.Data.csproj", "src/Fiap.CRM.Infra/"]
RUN dotnet restore "./src/Fiap.CRM.Api/Fiap.CRM.Api.csproj"
COPY . .
WORKDIR "/src/src/Fiap.CRM.Api"
RUN dotnet build "./Fiap.CRM.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.CRM.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.CRM.Api.dll"]