using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core CLI (`dotnet ef migrations`) at design time so that tooling never
/// depends on a committed secret or a live database. The connection string here is only
/// used to pick the provider for scaffolding — no connection is opened to add a migration.
/// At runtime the real connection string is supplied via the ConnectionStrings__CatalogDb
/// environment variable (see docker-compose / .env).
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__CatalogDb")
            ?? "Server=localhost;Database=CatalogDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CatalogDbContext(options);
    }
}
