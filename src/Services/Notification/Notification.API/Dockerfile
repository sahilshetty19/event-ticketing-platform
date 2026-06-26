# syntax=docker/dockerfile:1
# Multi-stage build. Build context is the repository root so the central
# Directory.Build.props / Directory.Packages.props are available to `dotnet restore`.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./

COPY src/BuildingBlocks/EventTicketing.Contracts/EventTicketing.Contracts.csproj src/BuildingBlocks/EventTicketing.Contracts/
COPY src/BuildingBlocks/EventTicketing.Messaging/EventTicketing.Messaging.csproj src/BuildingBlocks/EventTicketing.Messaging/
COPY src/Services/Notification/Notification.Domain/Notification.Domain.csproj                 src/Services/Notification/Notification.Domain/
COPY src/Services/Notification/Notification.Application/Notification.Application.csproj         src/Services/Notification/Notification.Application/
COPY src/Services/Notification/Notification.Infrastructure/Notification.Infrastructure.csproj  src/Services/Notification/Notification.Infrastructure/
COPY src/Services/Notification/Notification.API/Notification.API.csproj                        src/Services/Notification/Notification.API/
RUN dotnet restore src/Services/Notification/Notification.API/Notification.API.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Notification/ src/Services/Notification/
RUN dotnet publish src/Services/Notification/Notification.API/Notification.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Notification.API.dll"]
