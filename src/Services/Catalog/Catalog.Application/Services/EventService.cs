using Catalog.Application.Abstractions;
using Catalog.Application.Dtos;

namespace Catalog.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<EventDto>> GetEventsAsync(CancellationToken ct = default)
    {
        var events = await _repository.GetAllAsync(ct);
        return events.Select(e => new EventDto(
            e.Id, e.Name, e.Description, e.StartsAtUtc, e.Venue.Name, e.Venue.City)).ToList();
    }

    public async Task<EventDetailDto?> GetEventByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _repository.GetByIdAsync(id, ct);
        return e is null
            ? null
            : new EventDetailDto(e.Id, e.Name, e.Description, e.StartsAtUtc,
                e.Venue.Name, e.Venue.City, e.Seats.Count);
    }

    public async Task<IReadOnlyList<SeatDto>?> GetSeatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var exists = await _repository.GetByIdAsync(eventId, ct);
        if (exists is null) return null;

        var seats = await _repository.GetSeatsAsync(eventId, ct);
        return seats.Select(s => new SeatDto(
            s.Id, s.Section, s.Row, s.Number, s.Status.ToString())).ToList();
    }
}