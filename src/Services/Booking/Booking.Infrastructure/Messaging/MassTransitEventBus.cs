using Booking.Application.Abstractions;
using MassTransit;

namespace Booking.Infrastructure.Messaging;

/// <summary>Adapts the Application <see cref="IEventBus"/> port to MassTransit's publish endpoint.</summary>
public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => _publishEndpoint.Publish(message, ct);
}
