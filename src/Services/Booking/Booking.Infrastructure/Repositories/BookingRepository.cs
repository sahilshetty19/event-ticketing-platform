using Booking.Application.Abstractions;
using Booking.Domain.Entities;
using Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _db;

    public BookingRepository(BookingDbContext db) => _db = db;

    public Task<SeatBooking?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<SeatBooking?> GetActiveForSeatAsync(Guid eventId, Guid seatId, DateTime nowUtc, CancellationToken ct = default) =>
        _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b =>
                b.EventId == eventId &&
                b.SeatId == seatId &&
                (b.Status == BookingStatus.Confirmed ||
                 (b.Status == BookingStatus.Held && b.ExpiresAtUtc > nowUtc)), ct);

    public async Task AddAsync(SeatBooking booking, CancellationToken ct = default) =>
        await _db.Bookings.AddAsync(booking, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
