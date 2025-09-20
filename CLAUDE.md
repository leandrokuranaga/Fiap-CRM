# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Architecture

This is a .NET 8 CRM application built using Clean Architecture principles with the following layers:

### Layer Structure
- **API Layer** (`Fiap.CRM.Api`): Web API controllers, middleware, configuration
- **Application Layer** (`Fiap.CRM.Application`): Use cases, DTOs, validators, auth logic
- **Domain Layer** (`Fiap.CRM.Domain`): Core business entities and rules
- **Infrastructure Layer**: Multiple projects handling data persistence, external services, and cross-cutting concerns
  - `Fiap.CRM.Infra`: Data access and Entity Framework contexts
  - `Fiap.CRM.Infra.CrossCutting`: Shared utilities and configurations
  - `Fiap.CRM.Infra.CrossCutting.IoC`: Dependency injection container configuration
  - `Fiap.CRM.Infra.HandleBus`: Message bus handling
  - `Fiap.CRM.Infra.HostedService`: Background services
  - `Fiap.Contracts.Events`: Event contracts for messaging

### Key Technologies
- .NET 8 with ASP.NET Core Web API
- Entity Framework Core with MySQL
- Redis for caching
- Swagger/OpenAPI documentation
- Serilog for logging
- Prometheus metrics
- Health checks
- AWS Systems Manager for configuration
- Docker containerization
- gRPC support

## Development Commands

### Building and Testing
Since this project uses `.slnx` format, you need to create a temporary solution file first:

```bash
# Create temporary solution from .slnx
dotnet new sln -n Fiap.CRM
find . -name "*.csproj" -exec dotnet sln Fiap.CRM.sln add {} \;

# Restore dependencies
dotnet restore *.sln

# Build the solution
dotnet build *.sln --no-restore --configuration Release

# Run tests
dotnet test *.sln --no-build --configuration Release --verbosity normal

# Run specific test project
dotnet test test/Fiap.CRM.Unit.Tests/Fiap.CRM.Unit.Tests.csproj
dotnet test test/Fiap.CRM.BDD.Tests/Fiap.CRM.BDD.Tests/Fiap.CRM.BDD.Tests.csproj
```

### Running the Application
```bash
# Run the API project
dotnet run --project src/Fiap.CRM.Api/Fiap.CRM.Api.csproj

# With hot reload for development
dotnet watch run --project src/Fiap.CRM.Api/Fiap.CRM.Api.csproj
```

### Docker
```bash
# Build Docker image
docker build -t fiap-crm .

# Run container
docker run -p 8080:8080 fiap-crm
```

## Project Structure Guidelines

### API Layer (`src/Fiap.CRM.Api`)
- Contains controllers, middleware, and startup configuration
- Uses dependency injection configured in `Fiap.CRM.Infra.CrossCutting.IoC`
- Includes health checks for MySQL and Redis
- Swagger documentation with versioning support

### Application Layer (`src/Fiap.CRM.Application`)
- User management and authentication logic
- Input validation using FluentValidation
- Common utilities and shared application logic

### Configuration
- Uses `appsettings.json` for local configuration
- AWS Systems Manager Parameter Store for production configuration (path: `/myapp/prod/`)
- User secrets for development
- Environment-specific configuration files supported

### Testing
- Unit tests in `test/Fiap.CRM.Unit.Tests`
- BDD tests in `test/Fiap.CRM.BDD.Tests`
- All tests run via `dotnet test` command

### CI/CD Integration
- GitHub Actions workflows handle PR validation, releases, and deployments
- Automated Docker image publishing to Docker Hub
- AWS ECS deployment pipeline
- PR validation runs build and test suite automatically

## Important Notes

- Always create the temporary solution file before running build/test commands
- The project uses .NET 8 and requires the corresponding SDK
- Database migrations and seeding should be handled through Entity Framework Core
- Health checks are available at `/health` endpoint
- Metrics are exposed at `/metrics` for Prometheus scraping
- API documentation is available via Swagger UI when running in development mode