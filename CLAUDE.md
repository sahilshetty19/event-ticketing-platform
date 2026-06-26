# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Event-ticketing backend built as .NET 9 microservices. Only the **Catalog** service exists so far
(read-only browsing of events, venues, and seats). The code carries "Day N" comments — it is being
built incrementally as a learning/tutorial project, so expect features to be partially scaffolded.

## Commands

Run from the repository root.

```bash
# Build everything
dotnet build EventTicketing.sln

# Run the Catalog API (http://localhost:5264, https://localhost:7244; Swagger UI at /swagger in Development)
dotnet run --project src/Services/Catalog/Catalog.API

# EF Core migrations (Infrastructure holds the DbContext; API is the startup project)
dotnet ef migrations add <Name> \
  --project src/Services/Catalog/Catalog.Infrastructure \
  --startup-project src/Services/Catalog/Catalog.API
```

There is **no test project yet**. `Program.cs` already exposes `public partial class Program {}` for
future `WebApplicationFactory`-based integration tests.

## Known broken state (fix before anything else)

- The solution **does not compile**. `Catalog.Infrastructure` uses EF Core (`DbContext`,
  `UseSqlServer`, `IEntityTypeConfiguration`) but its `.csproj` is missing the package references.
  Add `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Design` to
  [Catalog.Infrastructure.csproj](src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj).
- **No migrations exist**, yet [Program.cs](src/Services/Catalog/Catalog.API/Program.cs) calls
  `db.Database.MigrateAsync()` on startup. Create an initial migration (command above) or the app
  will fail to start once it compiles.

## Runtime prerequisites

- A SQL Server instance reachable at `localhost:1433` (sa / `Your_password123`), per the `CatalogDb`
  connection string in [appsettings.json](src/Services/Catalog/Catalog.API/appsettings.json).
  Typically run via Docker (`mcr.microsoft.com/mssql/server`).
- On startup the API auto-runs migrations **and** seeds demo data
  ([CatalogDbSeeder](src/Services/Catalog/Catalog.Infrastructure/Persistence/CatalogDbSeeder.cs),
  idempotent — it no-ops if any venue exists).

## Architecture

Clean Architecture / DDD layering. Each `src/Services/<Service>` folder contains four projects with a
strict one-way dependency flow:

```
Catalog.API  →  Catalog.Infrastructure  →  Catalog.Application  →  Catalog.Domain
(controllers,    (EF Core, DbContext,       (DTOs, service +       (entities only,
 Program.cs,      repositories, seeder,       repository            no dependencies)
 Swagger)         DI registration)            interfaces, services)
```

- **Domain** — POCO entities only: `Event`, `Venue`, `Seat` (+ `SeatStatus` enum). No framework refs.
- **Application** — `record` DTOs ([CatalogDtos.cs](src/Services/Catalog/Catalog.Application/Dtos/CatalogDtos.cs)),
  abstractions (`IEventService`, `IEventRepository`), and `EventService` which maps entities → DTOs.
  Repository interfaces live here; implementations live in Infrastructure (dependency inversion).
- **Infrastructure** — `CatalogDbContext`, entity configs applied via
  `ApplyConfigurationsFromAssembly`, `EventRepository` (all queries `AsNoTracking` — this is a
  read-only catalog), and the seeder.
- **API** — thin controllers delegating to `IEventService`; endpoints under `api/events`.

### Conventions to follow when extending

- **DI per layer**: every layer exposes a static `DependencyInjection` class with an
  `Add<Layer>(...)` extension; `Program.cs` composes them (`AddApplication()`,
  `AddInfrastructure(config)`). Register new services there, not inline.
- **Async + `CancellationToken`**: every service/repository method takes and forwards a `ct`.
- **Entity configuration**: add a new `IEntityTypeConfiguration<T>` class in
  `Infrastructure/Persistence/Configurations` — it is picked up automatically by the assembly scan.
  Enums are persisted as strings (see `Seat.Status` → `HasConversion<string>()`).
- **Adding a new microservice**: mirror the Catalog four-project layout under `src/Services/<Name>`
  and register each project in `EventTicketing.sln` (solution folders nest under `src` → `Services`).
