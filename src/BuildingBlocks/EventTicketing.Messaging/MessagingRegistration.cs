using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Messaging;

public static class MessagingRegistration
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ (or the in-memory transport when RabbitMQ is not
    /// configured, e.g. local dev / tests). Applies a shared retry policy; once retries are
    /// exhausted MassTransit moves the message to the endpoint's <c>_error</c> queue, which is
    /// our dead-letter destination for poison messages.
    /// </summary>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration config,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            configureConsumers?.Invoke(x);

            var host = config["RabbitMq:Host"];
            if (!string.IsNullOrWhiteSpace(host))
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(host, config["RabbitMq:VirtualHost"] ?? "/", h =>
                    {
                        h.Username(config["RabbitMq:Username"] ?? "guest");
                        h.Password(config["RabbitMq:Password"] ?? "guest");
                    });

                    ConfigureResilience(cfg);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                x.UsingInMemory((ctx, cfg) =>
                {
                    ConfigureResilience(cfg);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        return services;
    }

    private static void ConfigureResilience(IBusFactoryConfigurator cfg)
    {
        // Retry transient failures a few times; exhausted messages are dead-lettered to *_error.
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
    }
}
