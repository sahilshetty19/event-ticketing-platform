using EventTicketing.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Abstractions;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PaymentDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("PaymentDb"),
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IEventBus, MassTransitEventBus>();

        // Messaging: Payment both consumes BookingConfirmed and publishes PaymentSucceeded.
        services.AddEventBus(config, "payment", x => x.AddConsumer<BookingConfirmedConsumer>());

        return services;
    }
}
