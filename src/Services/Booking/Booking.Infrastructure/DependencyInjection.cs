using Booking.Application.Abstractions;
using Booking.Infrastructure.Locking;
using Booking.Infrastructure.Messaging;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.Repositories;
using EventTicketing.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<BookingDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("BookingDb"),
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IBookingRepository, BookingRepository>();

        // Use a Redis-backed distributed lock when Redis is configured; otherwise fall back to an
        // in-process lock so the service still runs locally and in tests without Redis.
        var redisConnection = config.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect($"{redisConnection},abortConnect=false"));
            services.AddSingleton<IDistributedLock, RedisDistributedLock>();
        }
        else
        {
            services.AddSingleton<IDistributedLock, InMemoryDistributedLock>();
        }

        // Messaging: Booking publishes BookingConfirmed and consumes PaymentSucceeded.
        services.AddEventBus(config, "booking", x => x.AddConsumer<PaymentSucceededConsumer>());
        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
