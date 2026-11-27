# MPFT Clinic Space

An internal web app for staff to request clinical space, and for administrators to accept or reject those requests.

> Work in progress: built in vertical slices (see the GitHub issues). This README is completed in Step 7.

## Tech stack

| Layer | Choice |
|---|---|
| API | ASP.NET Core 11 (controllers), ProblemDetails |
| Data | EF Core 11, SQL Server LocalDB (SQL Server 2025), ASP.NET Core Identity schema |
| Client | React 19 + TypeScript, Vite 8, Tailwind CSS v4, React Router |
| Tests | xUnit v3 on Microsoft Testing Platform; SQLite in-memory for unit and API tests |

## Solution layout

```
src/
  ClinicSpace.Domain           entities (from Step 2)
  ClinicSpace.Application      use-case services, IAppDbContext abstraction
  ClinicSpace.Infrastructure   EF Core AppDbContext, Identity, migrations
  ClinicSpace.Api              HTTP API (controllers)
tests/
  ClinicSpace.Application.Tests  service tests against a real AppDbContext on SQLite in-memory
  ClinicSpace.Api.Tests          WebApplicationFactory tests with SQLite swapped in
client/                          Vite + React SPA
```

## Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download) (pinned in `global.json`)
- Node.js 22.22+ (developed on Node 26 LTS)
- SQL Server LocalDB, which ships with Visual Studio or the SQL Server Express installer

## Run locally

```bash
# 1. Restore the local EF Core CLI tool
dotnet tool restore

# 2. Create the database from migrations (LocalDB: database "ClinicSpace")
dotnet ef database update -p src/ClinicSpace.Infrastructure -s src/ClinicSpace.Api

# 3. Start the API (https://localhost:7196)
dotnet run --project src/ClinicSpace.Api --launch-profile https

# 4. In a second terminal, start the client (http://localhost:5173)
cd client
npm install
npm run dev
```

Open http://localhost:5173. The home page shows **API OK (db connected)**, which it fetches from `GET /api/health` through the Vite dev proxy.

To use full SQL Server or SQL Server Express instead of LocalDB, change only `ConnectionStrings:DefaultConnection` in `src/ClinicSpace.Api/appsettings.Development.json` (for example `Server=.\SQLEXPRESS;...`).

## Tests

```bash
dotnet test
```

Tests need no database server: they use SQLite in-memory.

## Decisions so far

- **.NET 11 (STS)** rather than .NET 10 (LTS): .NET 11 is the latest stable release. The trade-off is a shorter support window (18 months instead of 3 years).
- **LocalDB** for development: it runs the same SQL Server engine with nothing extra to install. Migrations are SQL Server-specific; tests build their schema with `EnsureCreated()` on SQLite.
- **`IAppDbContext` in Application:** services depend on an abstraction rather than the concrete context in Infrastructure, so the dependency points inwards.
