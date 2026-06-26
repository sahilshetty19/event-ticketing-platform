# syntax=docker/dockerfile:1
# Multi-stage build. Build context is the repository root so the central
# Directory.Build.props / Directory.Packages.props are available to `dotnet restore`.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./

COPY src/Services/Booking/Booking.Domain/Booking.Domain.csproj                 src/Services/Booking/Booking.Domain/
COPY src/Services/Booking/Booking.Application/Booking.Application.csproj         src/Services/Booking/Booking.Application/
COPY src/Services/Booking/Booking.Infrastructure/Booking.Infrastructure.csproj  src/Services/Booking/Booking.Infrastructure/
COPY src/Services/Booking/Booking.API/Booking.API.csproj                        src/Services/Booking/Booking.API/
RUN dotnet restore src/Services/Booking/Booking.API/Booking.API.csproj

COPY src/Services/Booking/ src/Services/Booking/
RUN dotnet publish src/Services/Booking/Booking.API/Booking.API.csproj \
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
ENTRYPOINT ["dotnet", "Booking.API.dll"]
