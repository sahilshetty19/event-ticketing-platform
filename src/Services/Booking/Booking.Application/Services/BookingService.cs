using Booking.Application.Abstractions;
using Booking.Application.Dtos;
using Booking.Domain.Entities;
using Booking.Domain.Exceptions;
using EventTicketing.Contracts;

namespace Booking.Application.Services;

public class BookingService : IBookingService
{
    // A hold reserves a seat for a limited window before it must be confirmed.
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);
    // How long the seat-level lock is held while we check availability and write the hold.
    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);
    private const decimal DefaultTicketPrice = 50m;

    private readonly IBookingRepository _repository;
    private readonly IDistributedLock _distributedLock;
    private readonly IEventBus _eventBus;

    public BookingService(IBookingRepository repository, IDistributedLock distributedLock, IEventBus eventBus)
    {
        _repository = repository;
        _distributedLock = distributedLock;
        _eventBus = eventBus;
    }

    public async Task<HoldResult> HoldAsync(HoldSeatRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var lockKey = SeatLockKey(request.EventId, request.SeatId);

        // The distributed lock serializes concurrent hold attempts for the SAME seat across
        // all Booking instances, closing the check-then-write race that causes double-booking.
        await using var handle = await _distributedLock.AcquireAsync(lockKey, LockExpiry, ct);
        if (handle is null)
            return new HoldResult(HoldOutcome.LockNotAcquired, null);

        var existing = await _repository.GetActiveForSeatAsync(request.EventId, request.SeatId, now, ct);
        if (existing is not null)
            return new HoldResult(HoldOutcome.SeatUnavailable, null);

        var amount = request.Amount > 0 ? request.Amount : DefaultTicketPrice;
        var booking = SeatBooking.Hold(request.EventId, request.SeatId, request.CustomerId, amount, HoldDuration, now);

        await _repository.AddAsync(booking, ct);
        await _repository.SaveChangesAsync(ct);

        return new HoldResult(HoldOutcome.Success, ToResponse(booking));
    }

    public async Task<ConfirmResult> ConfirmAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _repository.GetByIdAsync(bookingId, ct);
        if (booking is null)
            return new ConfirmResult(ConfirmOutcome.NotFound, null, null);

        try
        {
            booking.Confirm(DateTime.UtcNow);
        }
        catch (InvalidBookingStateException ex)
        {
            return new ConfirmResult(ConfirmOutcome.InvalidState, null, ex.Message);
        }

        await _repository.SaveChangesAsync(ct);

        // Notify the rest of the system. Payment and Notification react to this asynchronously.
        await _eventBus.PublishAsync(new BookingConfirmed(
            booking.Id, booking.EventId, booking.SeatId, booking.CustomerId,
            booking.Amount, booking.ConfirmedAtUtc!.Value), ct);

        return new ConfirmResult(ConfirmOutcome.Success, ToResponse(booking), null);
    }

    public async Task<BookingResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await _repository.GetByIdAsync(id, ct);
        return booking is null ? null : ToResponse(booking);
    }

    private static string SeatLockKey(Guid eventId, Guid seatId) => $"seat-lock:{eventId}:{seatId}";

    private static BookingResponse ToResponse(SeatBooking b) => new(
        b.Id, b.EventId, b.SeatId, b.CustomerId, b.Amount,
        b.Status.ToString(), b.HeldAtUtc, b.ExpiresAtUtc, b.ConfirmedAtUtc);
}
