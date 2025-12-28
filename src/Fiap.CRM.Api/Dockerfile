# =============================================================================
# DOCKERFILE OTIMIZADO - CRM API
# Imagem base: Alpine (~100MB vs ~200MB debian)
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar apenas os .csproj primeiro (melhor cache de layers)
COPY ["src/Fiap.CRM.Api/Fiap.CRM.Api.csproj", "src/Fiap.CRM.Api/"]
COPY ["src/Fiap.CRM.Application/Fiap.CRM.Application.csproj", "src/Fiap.CRM.Application/"]
COPY ["src/Fiap.CRM.Domain/Fiap.CRM.Domain.csproj", "src/Fiap.CRM.Domain/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting/Fiap.CRM.Infra.CrossCutting.csproj", "src/Fiap.CRM.Infra.CrossCutting/"]
COPY ["src/Fiap.CRM.Infra.HostedService/Fiap.CRM.Infra.HostedService.csproj", "src/Fiap.CRM.Infra.HostedService/"]
COPY ["src/Fiap.Contracts.Events/Fiap.Contracts.Events.csproj", "src/Fiap.Contracts.Events/"]
COPY ["src/Fiap.CRM.Infra.CrossCutting.IoC/Fiap.CRM.Infra.CrossCutting.IoC.csproj", "src/Fiap.CRM.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.CRM.Infra.HandleBus/Fiap.CRM.Infra.Bus.csproj", "src/Fiap.CRM.Infra.HandleBus/"]
COPY ["src/Fiap.CRM.Infra/Fiap.CRM.Infra.Data.csproj", "src/Fiap.CRM.Infra/"]

# Restore com cache
RUN dotnet restore "src/Fiap.CRM.Api/Fiap.CRM.Api.csproj"

# Copiar código fonte
COPY . .

# Build e publish
WORKDIR "/src/src/Fiap.CRM.Api"
RUN dotnet publish "Fiap.CRM.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =============================================================================
# Runtime stage - Imagem final otimizada
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Labels para metadata
LABEL maintainer="FIAP Team" \
      app="crm-api" \
      version="1.0"

# Instalar dependências necessárias e criar usuário não-root
RUN apk add --no-cache \
        icu-libs \
        tzdata \
    && addgroup -g 1000 appgroup \
    && adduser -u 1000 -G appgroup -s /bin/sh -D appuser

# Configurar timezone
ENV TZ=America/Sao_Paulo \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app

# Copiar artefatos do build
COPY --from=build --chown=appuser:appgroup /app/publish .

# Usar usuário não-root
USER appuser

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Fiap.CRM.Api.dll"]
