using EventTicketing.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Abstractions;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Notifications;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Repositories;

namespace Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<NotificationDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("NotificationDb"),
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationSender, LoggingNotificationSender>();

        // Messaging: register the consumer with MassTransit so it binds to a RabbitMQ queue.
        services.AddEventBus(config, x => x.AddConsumer<BookingConfirmedConsumer>());

        return services;
    }
}
