using Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests;

/// <summary>
/// Boots the real Catalog API in-process but swaps SQL Server for the EF Core in-memory
/// provider so the test needs no database. The app's startup seeding still runs (via
/// EnsureCreated), so the seeded venue/event/seats are available to assert against.
/// </summary>
public class CatalogApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Drop every DbContext / options registration left by AddInfrastructure (including
            // EF Core 9's IDbContextOptionsConfiguration<T>, matched by name so we don't depend
            // on an internal type). Otherwise both SQL Server and InMemory providers stay
            // registered and EF refuses to start.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(CatalogDbContext) ||
                (d.ServiceType.FullName?.Contains("DbContextOptions") ?? false)).ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            // ...and replace it with the in-memory provider.
            services.AddDbContext<CatalogDbContext>(o =>
                o.UseInMemoryDatabase("catalog-integration-tests"));
        });
    }
}
