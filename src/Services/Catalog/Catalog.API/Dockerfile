# syntax=docker/dockerfile:1
# Multi-stage build. Build context is the repository root so the central
# Directory.Build.props / Directory.Packages.props are available to `dotnet restore`.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy central build files first for better layer caching.
COPY Directory.Build.props Directory.Packages.props ./

# Copy only the csproj files and restore (cached unless a csproj changes).
COPY src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj         src/Services/Catalog/Catalog.Domain/
COPY src/Services/Catalog/Catalog.Application/Catalog.Application.csproj src/Services/Catalog/Catalog.Application/
COPY src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj src/Services/Catalog/Catalog.Infrastructure/
COPY src/Services/Catalog/Catalog.API/Catalog.API.csproj                src/Services/Catalog/Catalog.API/
RUN dotnet restore src/Services/Catalog/Catalog.API/Catalog.API.csproj

# Copy the rest of the sources and publish.
COPY src/Services/Catalog/ src/Services/Catalog/
RUN dotnet publish src/Services/Catalog/Catalog.API/Catalog.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl is used by the docker-compose healthcheck to probe /health.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
