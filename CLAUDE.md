# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Cloud-native event-ticketing backend: **four .NET 9 microservices** (Catalog, Booking, Payment,
Notification) behind a **YARP API gateway**, using SQL Server (database-per-service), Redis,
and RabbitMQ (MassTransit). Backend only. See [README.md](README.md) for the full architecture,
endpoint list, and design-decision writeup.

## Commands

Run from the repository root.

```bash
# Build / test the whole solution
dotnet build EventTicketing.sln
dotnet test EventTicketing.sln                 # 34 tests; no external infra needed

# Single test project / single test
dotnet test tests/Booking.IntegrationTests/Booking.IntegrationTests.csproj
dotnet test tests/Booking.UnitTests/Booking.UnitTests.csproj --filter "FullyQualifiedName~Confirm"

# Run the entire system (gateway + 4 services + SQL Server + Redis + RabbitMQ)
cp .env.example .env        # first time only (.env is gitignored)
docker compose up --build

# EF Core migration for a service (Infrastructure holds the DbContext; API is startup)
dotnet ef migrations add <Name> \
  --project src/Services/<Service>/<Service>.Infrastructure \
  --startup-project src/Services/<Service>/<Service>.API \
  --output-dir Persistence/Migrations
```

The EF CLI tools must be v9+ (`dotnet tool update --global dotnet-ef --version 9.*`).

## Architecture & conventions

Each `src/Services/<Service>` has four projects with a strict one-way dependency flow
(`API → Infrastructure → Application → Domain`); Domain has no dependencies. Shared code is under
`src/BuildingBlocks` (`EventTicketing.Contracts` = integration-event records;
`EventTicketing.Messaging` = shared MassTransit/RabbitMQ setup). The gateway is
`src/ApiGateway/ApiGateway`.

- **Per-layer DI:** every layer exposes a `DependencyInjection` static class with an `Add<Layer>(...)`
  extension; `Program.cs` composes `AddApplication()` + `AddInfrastructure(config)`.
- **Ports & adapters:** cross-cutting concerns are interfaces in Application with implementations in
  Infrastructure — `ICacheService` (Catalog), `IDistributedLock` + `IEventBus` (Booking),
  `IEventBus` (Payment). Each has an in-memory/test-friendly fallback selected when Redis/RabbitMQ
  are not configured, which is what lets tests run with no external infrastructure.
- **Messaging:** publish/consume `BookingConfirmed` and `PaymentSucceeded` via MassTransit. Consumers
  must be **idempotent** (at-least-once delivery); retries then dead-letter to `*_error` queues.
- **Persistence:** EF Core, enums stored as strings (`HasConversion<string>()`), migrations run on
  startup (relational only; tests use the in-memory provider). Each service has a design-time
  `DbContextFactory` so `dotnet ef` needs no live DB or secret.
- **Async + `CancellationToken`** on every service/repository method.
- **No hardcoded secrets:** connection strings / broker creds come from env vars (`.env`, gitignored;
  `.env.example` is committed).
- **Central Package Management:** package versions live in `Directory.Packages.props`; csproj
  `PackageReference`s are versionless. Common TFM/Nullable settings are in `Directory.Build.props`.
  Framework-tied packages (EF Core, AspNetCore.Mvc.Testing) are pinned to 9.0.x for the net9.0 target.

### Adding a new microservice
Mirror the four-project layout under `src/Services/<Name>`, register each project in
`EventTicketing.sln`, add a Dockerfile (build context = repo root so the central props files
resolve), and add the service + healthcheck to `docker-compose.yml`.

## Tests

`tests/*` — xUnit + Moq unit tests per service, plus `WebApplicationFactory` integration tests
(Catalog read endpoints; Booking hold→confirm→event-published using the MassTransit in-memory test
harness). xUnit needs an explicit `using Xunit;` (no global usings). The integration factories swap
SQL Server → EF in-memory and RabbitMQ → MassTransit test harness by removing the real registrations
in `ConfigureTestServices`.
