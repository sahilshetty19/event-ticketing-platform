using Catalog.Application.Abstractions;
using Catalog.Application.Dtos;

namespace Catalog.Application.Services;

public class EventService : IEventService
{
    // Catalog data is read-heavy and changes rarely, so a short TTL keeps reads fast
    // while bounding staleness. Write paths (e.g. a seat being booked) evict explicitly.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private const string AllEventsKey = "catalog:events:all";
    private static string EventKey(Guid id) => $"catalog:event:{id}";
    private static string SeatsKey(Guid id) => $"catalog:event:{id}:seats";

    private readonly IEventRepository _repository;
    private readonly ICacheService _cache;

    public EventService(IEventRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<EventDto>> GetEventsAsync(CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(AllEventsKey, async token =>
        {
            var events = await _repository.GetAllAsync(token);
            return events.Select(e => new EventDto(
                e.Id, e.Name, e.Description, e.StartsAtUtc, e.Venue.Name, e.Venue.City)).ToList();
        }, CacheTtl, ct);

        return result ?? [];
    }

    public async Task<EventDetailDto?> GetEventByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _cache.GetOrSetAsync(EventKey(id), async token =>
        {
            var e = await _repository.GetByIdAsync(id, token);
            return e is null
                ? null
                : new EventDetailDto(e.Id, e.Name, e.Description, e.StartsAtUtc,
                    e.Venue.Name, e.Venue.City, e.Seats.Count);
        }, CacheTtl, ct);
    }

    public async Task<IReadOnlyList<SeatDto>?> GetSeatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var exists = await _repository.GetByIdAsync(eventId, ct);
        if (exists is null) return null;

        var result = await _cache.GetOrSetAsync(SeatsKey(eventId), async token =>
        {
            var seats = await _repository.GetSeatsAsync(eventId, token);
            return seats.Select(s => new SeatDto(
                s.Id, s.Section, s.Row, s.Number, s.Status.ToString())).ToList();
        }, CacheTtl, ct);

        return result ?? [];
    }
}
