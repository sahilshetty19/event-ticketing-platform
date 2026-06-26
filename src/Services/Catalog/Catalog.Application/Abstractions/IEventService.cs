using Catalog.Application.Dtos;

namespace Catalog.Application.Abstractions;

public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetEventsAsync(CancellationToken ct = default);
    Task<EventDetailDto?> GetEventByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SeatDto>?> GetSeatsAsync(Guid eventId, CancellationToken ct = default);
}