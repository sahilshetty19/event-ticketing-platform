# syntax=docker/dockerfile:1
# Multi-stage build. Build context is the repository root so the central
# Directory.Build.props / Directory.Packages.props are available to `dotnet restore`.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./

COPY src/BuildingBlocks/EventTicketing.Contracts/EventTicketing.Contracts.csproj src/BuildingBlocks/EventTicketing.Contracts/
COPY src/BuildingBlocks/EventTicketing.Messaging/EventTicketing.Messaging.csproj src/BuildingBlocks/EventTicketing.Messaging/
COPY src/Services/Payment/Payment.Domain/Payment.Domain.csproj                 src/Services/Payment/Payment.Domain/
COPY src/Services/Payment/Payment.Application/Payment.Application.csproj         src/Services/Payment/Payment.Application/
COPY src/Services/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj  src/Services/Payment/Payment.Infrastructure/
COPY src/Services/Payment/Payment.API/Payment.API.csproj                        src/Services/Payment/Payment.API/
RUN dotnet restore src/Services/Payment/Payment.API/Payment.API.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Payment/ src/Services/Payment/
RUN dotnet publish src/Services/Payment/Payment.API/Payment.API.csproj \
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
ENTRYPOINT ["dotnet", "Payment.API.dll"]
