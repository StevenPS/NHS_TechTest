# MPFT Clinic Space

An internal web app for staff to request clinical space, and for administrators to accept or reject those requests.

> Work in progress: built in vertical slices (see the GitHub issues). This README is completed in Step 7.

## Tech stack

| Layer | Choice |
|---|---|
| API | ASP.NET Core 11 (controllers), ProblemDetails |
| Data | EF Core 11, SQL Server LocalDB (SQL Server 2025), ASP.NET Core Identity schema |
| Client | React 19 + TypeScript, Vite 8, Tailwind CSS v4, React Router |
| Tests | xUnit v3 on Microsoft Testing Platform; SQLite in-memory for service tests |

## Solution layout

```
src/
  ClinicSpace.Domain           entities, service contracts (IAuthService, ICurrentUserService, IHealthService) and their models
  ClinicSpace.Infrastructure   EF Core AppDbContext, HealthService, Identity (AuthService, CurrentUserService, DevDataSeeder), migrations
  ClinicSpace.Api              HTTP API (controllers)
tests/
  ClinicSpace.Infrastructure.Tests  service tests (health, Identity services, seeder) on SQLite in-memory
client/                          Vite + React SPA
```

## Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download) (pinned in `global.json`)
- Node.js 22.22+ (developed on Node 26 LTS)
- SQL Server LocalDB, which ships with Visual Studio or the SQL Server Express installer

## Scripts

PowerShell scripts in [`scripts/`](scripts/) wrap the common tasks. They work in Windows PowerShell 5.1 and in PowerShell 7 (`pwsh`):

| Script | What it does |
|---|---|
| `.\scripts\build.ps1 [-Configuration Release]` | Restores tools, builds the .NET solution, installs client packages if needed, builds the client |
| `.\scripts\test.ps1 [-SkipClient]` | Runs `dotnet test`, then lints and type-checks the client |
| `.\scripts\start.ps1 [-SkipMigrations]` | Applies migrations, starts the API in the background and the client in the foreground; Ctrl+C stops both |

If script execution is blocked on your machine, run a script with `powershell -ExecutionPolicy Bypass -File .\scripts\start.ps1`.

## Run locally (manual steps)

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

Open http://localhost:5173 and sign in with one of the demo accounts below. The home page greets you and shows **API OK (db connected)**, which it fetches from `GET /api/health` through the Vite dev proxy.

## Demo accounts (development only)

When the API starts in the Development environment it seeds the `Admin` and `Requester` roles and the 14 users from the **Users** sheet of [`docs/Practical Exercise Data - Applications Developer B6.xlsx`](docs/). Seeding only adds what is missing, so restarting is safe. Sign in with the **email address**.

| Role | Accounts | Password |
|---|---|---|
| Admin | `pat.jones@example.nhs.uk`, `aman.singh@…`, `sam.taylor@…`, `jordan.brown@…` | `AdminPass!23` |
| Requester | `riley.green@example.nhs.uk`, `alex.evans@…`, `morgan.wood@…`, `sara.khan@…`, `jamie.clarke@…`, `lee.wilson@…`, `casey.hughes@…`, `taylor.patel@…`, `drew.cooper@…`, `kai.morris@…` | `Password123!` |

All accounts use the `@example.nhs.uk` domain. These passwords come with the exercise data and are for local development only; the seeder never runs outside Development.

To use full SQL Server or SQL Server Express instead of LocalDB, change only `ConnectionStrings:DefaultConnection` in `src/ClinicSpace.Api/appsettings.Development.json` (for example `Server=.\SQLEXPRESS;...`).

## Tests

```bash
dotnet test
```

Tests cover the services only (no controller or HTTP tests). They need no database server: they use SQLite in-memory.

## Decisions so far

- **.NET 11 (STS)** rather than .NET 10 (LTS): .NET 11 is the latest stable release. The trade-off is a shorter support window (18 months instead of 3 years).
- **LocalDB** for development: it runs the same SQL Server engine with nothing extra to install. Migrations are SQL Server-specific; tests build their schema with `EnsureCreated()` on SQLite.
- **No separate Application layer:** service contracts live in Domain and their implementations in Infrastructure, next to the `AppDbContext` they use. An extra project and an `IAppDbContext` abstraction added indirection without isolating anything.

### Authentication

- **ASP.NET Core Identity with a cookie**, as the handout asks for simple username and password sign-in. Passwords are stored as Identity's salted PBKDF2 hashes. There is no self-registration: users come only from the seeder. A live deployment would use Microsoft Entra ID instead.
- **Sign in by email.** The user name is set to the email address. A failed sign-in always returns the same `401` "Invalid email or password." so it does not reveal which accounts exist.
- **Roles** are `Admin` and `Requester` (the names used in the exercise data), stored in `AspNetRoles`. Each user has one role.
- **Identity's default password policy; lockout is off.** Both are deliberate simplifications for the exercise, with Entra owning account protection in a live deployment.
- **Cookie:** `.ClinicSpace.Auth`, HttpOnly, Secure, SameSite=Strict, Identity's default 14-day sliding expiry. The API answers `401`/`403` instead of redirecting, and the SPA decides where to go.
- **CSRF:** the SPA calls `GET /api/auth/antiforgery`, which sets a readable `XSRF-TOKEN` cookie, and echoes it in an `X-XSRF-TOKEN` header. A global filter rejects any POST/PUT/PATCH/DELETE without a valid token (`400`), login included. The token is tied to the user, so the SPA gets a fresh one after signing in or out.
- **Endpoints:** `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`.
- **SPA:** every page except `/sign-in` needs a signed-in user; `<RequireRole>` guards role-only pages. After sign-in the user returns to the page they asked for, but only if it is a path on this site.
