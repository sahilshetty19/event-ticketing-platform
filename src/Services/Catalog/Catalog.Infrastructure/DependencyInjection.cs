using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CatalogDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("CatalogDb"),
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IEventRepository, EventRepository>();
        return services;
    }
}