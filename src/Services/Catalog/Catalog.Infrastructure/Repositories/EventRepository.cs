using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly CatalogDbContext _db;

    public EventRepository(CatalogDbContext db) => _db = db;

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Events.AsNoTracking()
            .Include(e => e.Venue)
            .OrderBy(e => e.StartsAtUtc)
            .ToListAsync(ct);

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Events.AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.Seats)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Seat>> GetSeatsAsync(Guid eventId, CancellationToken ct = default) =>
        await _db.Seats.AsNoTracking()
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.Section).ThenBy(s => s.Row).ThenBy(s => s.Number)
            .ToListAsync(ct);
}