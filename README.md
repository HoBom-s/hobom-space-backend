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

- API Docs (Scalar): `http://localhost:5254/scalar/v1`
- OpenAPI Spec: `http://localhost:5254/openapi/v1.json`

## Testing

```bash
# Unit tests only (no Docker required)
dotnet test --filter "Category!=Integration"

# All tests including integration (Docker required)
dotnet test
```

Integration tests use Testcontainers to spin up a PostgreSQL container automatically. Docker Desktop must be running.

## Code Style

This project uses `.editorconfig` for code style enforcement.

```bash
# Check formatting
dotnet format --verify-no-changes

# Auto-fix formatting
dotnet format
```

## CI

GitHub Actions runs on PR to `develop`:

- Build
- Unit tests
- Integration tests (Docker)
- Migration pending check
- Format check
