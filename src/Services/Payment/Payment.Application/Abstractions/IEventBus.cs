namespace Payment.Application.Abstractions;

/// <summary>Outbound integration-event publisher (port); MassTransit implementation in Infrastructure.</summary>
public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
}
