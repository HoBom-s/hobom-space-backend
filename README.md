# hobom-space-backend

Backend API server for HoBom Space — a Confluence-style document management service.

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- PostgreSQL (EF Core + Npgsql)
- Clean Architecture (Domain → Application → Infrastructure → Api)
- Serilog (structured logging)

## Project Structure

```
src/
├── HobomSpace.Api/              # HTTP endpoints, middleware, configuration
├── HobomSpace.Application/      # Services, port interfaces
├── HobomSpace.Domain/           # Domain entities, exceptions
└── HobomSpace.Infrastructure/   # DbContext, repository implementations
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL

### Local Setup

1. Configure local settings:

```bash
cp src/HobomSpace.Api/appsettings.Local.example.json src/HobomSpace.Api/appsettings.Local.json
# Edit ConnectionStrings and Security:ApiKey in appsettings.Local.json
```

2. Run EF Core migrations:

```bash
dotnet ef migrations add InitialCreate -p src/HobomSpace.Infrastructure -s src/HobomSpace.Api
dotnet ef database update -p src/HobomSpace.Infrastructure -s src/HobomSpace.Api
```

3. Run the server:

```bash
dotnet run --project src/HobomSpace.Api
```

Server runs at `http://localhost:5254`.

## Code Style

This project uses `.editorconfig` for code style enforcement.

```bash
# Check formatting
dotnet format --verify-no-changes

# Auto-fix formatting
dotnet format
```

## CI

GitHub Actions runs on every push and PR to `develop`:

- Build
- Format check (`dotnet format --verify-no-changes`)
