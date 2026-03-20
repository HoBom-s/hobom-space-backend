# hobom-space-backend

Backend API server for HoBom Space — a Confluence-style document collaboration platform.

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- PostgreSQL (EF Core + Npgsql)
- gRPC (dual-port: REST 8080, gRPC 50052)
- Clean Architecture (Domain → Application → Infrastructure → Api)
- Serilog (structured logging with correlation ID)

## Project Structure

```
src/
├── HobomSpace.Api/              # Endpoints, middleware, gRPC services, background jobs
├── HobomSpace.Application/      # Services, port interfaces, helpers
├── HobomSpace.Domain/           # Entities, domain exceptions
└── HobomSpace.Infrastructure/   # DbContext, EF configurations, repositories
tests/
└── HobomSpace.Tests/            # Unit tests (NSubstitute) + Integration tests (Testcontainers)
```

## Features

| Feature | Description |
|---------|-------------|
| **Spaces** | Space CRUD with unique key |
| **Pages** | Page tree (parent-child), CRUD, position ordering |
| **Page Move / Copy** | Cross-space move, single-page copy with `[Copy]` prefix |
| **Soft Delete (Trash)** | Soft delete with 30-day auto-purge, restore, permanent delete |
| **Labels** | Space-scoped labels, attach/detach to pages, query pages by label |
| **Version History** | Snapshot-based page versioning, restore to any version |
| **Version Diff** | LCS-based line diff between two versions |
| **Comments** | Threaded comments (nested replies) on pages |
| **Search** | Full-text search across pages (global + per-space) |
| **Outbox Pattern** | Transactional outbox for event-driven integration |
| **Error Capture** | Client error event reporting and querying |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL

### Local Setup

1. Configure local settings:

```bash
cp src/HobomSpace.Api/appsettings.Local.example.json src/HobomSpace.Api/appsettings.Local.json
# Edit ConnectionStrings and Security:ApiKey
```

2. Run the server (migrations apply automatically on startup):

```bash
dotnet run --project src/HobomSpace.Api
```

Server runs at `http://localhost:8080`.

- API Docs (Scalar): `http://localhost:8080/scalar/v1`
- OpenAPI Spec: `http://localhost:8080/openapi/v1.json`
- Health Check: `http://localhost:8080/health/live`

## Testing

```bash
# Unit tests only (no Docker required)
dotnet test --filter "Category!=Integration"

# All tests including integration (Docker required)
dotnet test
```

Integration tests use Testcontainers to spin up a PostgreSQL container automatically.

## CI

GitHub Actions runs on PR to `develop`:

- Build (Release)
- Unit tests with **code coverage report** (PR comment)
- Integration tests (Docker)
- Migration pending check
- Format check

## Code Style

```bash
dotnet format --verify-no-changes   # Check
dotnet format                       # Auto-fix
```
