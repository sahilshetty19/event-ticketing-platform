namespace Booking.Application.Abstractions;

/// <summary>
/// Outbound integration-event publisher (port). The MassTransit/RabbitMQ implementation lives
/// in Infrastructure so Application stays unaware of the messaging transport.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
}
