using Booking.Domain.Entities;

namespace Booking.Application.Abstractions;

public interface IBookingRepository
{
    Task<SeatBooking?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns the booking that currently occupies a seat: a confirmed booking, or a hold
    /// that has not yet expired. Used to reject double-booking.
    /// </summary>
    Task<SeatBooking?> GetActiveForSeatAsync(Guid eventId, Guid seatId, DateTime nowUtc, CancellationToken ct = default);

    Task AddAsync(SeatBooking booking, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
