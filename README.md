# hobom-space-backend

Backend API server for HoBom Space — a Confluence-style document management service.

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- PostgreSQL (EF Core + Npgsql)
- Clean Architecture (Domain → Application → Infrastructure → Api)

## Project Structure

```
src/
├── HobomSpace.Api/              # HTTP endpoints, configuration
├── HobomSpace.Application/      # Use cases, port interfaces
├── HobomSpace.Domain/           # Domain entities
└── HobomSpace.Infrastructure/   # DbContext, repository implementations
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL

### Local Setup

1. Configure DB connection:

```bash
cp src/HobomSpace.Api/appsettings.Local.example.json src/HobomSpace.Api/appsettings.Local.json
# Edit ConnectionStrings in appsettings.Local.json
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
