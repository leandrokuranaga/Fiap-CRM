# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia toda a pasta src (inclui todos os .csproj)
COPY src/ ./ 

# Restaura dependências e compila
WORKDIR /src/Fiap.CRM.Api
RUN dotnet restore "Fiap.CRM.Api.csproj"
RUN dotnet build "Fiap.CRM.Api.csproj" -c Release -o /app/build

# Estágio de publicação
FROM build AS publish
RUN dotnet publish "Fiap.CRM.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.CRM.Api.dll"]
