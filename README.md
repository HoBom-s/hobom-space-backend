# hobom-space-backend

HoBom Space 서비스의 백엔드 API 서버. Confluence 스타일의 문서 관리 기능을 제공한다.

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- PostgreSQL (EF Core + Npgsql)
- Clean Architecture (Domain → Application → Infrastructure → Api)

## Project Structure

```
src/
├── HobomSpace.Api/              # HTTP 엔드포인트, 설정
├── HobomSpace.Application/      # 유스케이스, 포트 인터페이스
├── HobomSpace.Domain/           # 도메인 엔티티
└── HobomSpace.Infrastructure/   # DB 컨텍스트, 리포지토리 구현
```

## API Endpoints

### Spaces

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/spaces` | Space 생성 |
| GET | `/api/v1/spaces` | Space 목록 조회 |
| GET | `/api/v1/spaces/{key}` | Space 단건 조회 |
| PUT | `/api/v1/spaces/{key}` | Space 수정 |
| DELETE | `/api/v1/spaces/{key}` | Space 삭제 |

### Pages

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/spaces/{spaceKey}/pages` | Page 생성 |
| GET | `/api/v1/spaces/{spaceKey}/pages` | Page 트리 조회 |
| GET | `/api/v1/spaces/{spaceKey}/pages/{pageId}` | Page 단건 조회 |
| PUT | `/api/v1/spaces/{spaceKey}/pages/{pageId}` | Page 수정 |
| DELETE | `/api/v1/spaces/{spaceKey}/pages/{pageId}` | Page 삭제 |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL

### Local Setup

1. DB 커넥션 설정:

```bash
cp src/HobomSpace.Api/appsettings.Local.example.json src/HobomSpace.Api/appsettings.Local.json
# appsettings.Local.json에서 ConnectionStrings 수정
```

2. EF Core 마이그레이션 실행:

```bash
dotnet ef migrations add InitialCreate -p src/HobomSpace.Infrastructure -s src/HobomSpace.Api
dotnet ef database update -p src/HobomSpace.Infrastructure -s src/HobomSpace.Api
```

3. 서버 실행:

```bash
dotnet run --project src/HobomSpace.Api
```

서버는 `http://localhost:5254` 에서 실행된다.

## Database

`hobom-internal-backend`와 동일한 PostgreSQL 서버를 사용하며, `space` 스키마로 테이블을 격리한다.

| Table | Schema | Description |
|-------|--------|-------------|
| `spaces` | `space` | Space (워크스페이스) |
| `pages` | `space` | Page (문서, 트리 구조) |
