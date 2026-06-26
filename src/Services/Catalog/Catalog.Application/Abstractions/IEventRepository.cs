using Catalog.Domain.Entities;

namespace Catalog.Application.Abstractions;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Seat>> GetSeatsAsync(Guid eventId, CancellationToken ct = default);
}