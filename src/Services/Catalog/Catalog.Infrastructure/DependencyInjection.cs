using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Caching;
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

        // Cache-aside: use Redis when configured, otherwise an in-memory distributed cache
        // so the service still runs locally / in tests without a Redis dependency.
        var redisConnection = config.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
            services.AddStackExchangeRedisCache(opt => opt.Configuration = redisConnection);
        else
            services.AddDistributedMemoryCache();

        services.AddSingleton<ICacheService, DistributedCacheService>();

        return services;
    }
}
