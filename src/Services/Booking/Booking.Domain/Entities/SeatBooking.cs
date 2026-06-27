using Booking.Domain.Exceptions;

namespace Booking.Domain.Entities;

/// <summary>
/// Aggregate root representing a customer's claim on a single seat for an event.
/// State transitions are guarded here so invariants hold regardless of caller.
/// </summary>
public class SeatBooking
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid SeatId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime HeldAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    public bool IsPaid => PaidAtUtc is not null;

    // Required by EF Core.
    private SeatBooking() { }

    /// <summary>Creates a new time-boxed hold on a seat.</summary>
    public static SeatBooking Hold(
        Guid eventId, Guid seatId, Guid customerId, decimal amount,
        TimeSpan holdDuration, DateTime nowUtc)
    {
        if (amount < 0)
            throw new InvalidBookingStateException("Amount cannot be negative.");
        if (holdDuration <= TimeSpan.Zero)
            throw new InvalidBookingStateException("Hold duration must be positive.");

        return new SeatBooking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            SeatId = seatId,
            CustomerId = customerId,
            Amount = amount,
            Status = BookingStatus.Held,
            HeldAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(holdDuration)
        };
    }

    public bool IsExpired(DateTime nowUtc) => Status == BookingStatus.Held && nowUtc >= ExpiresAtUtc;

    /// <summary>Confirms a live hold. Throws if the booking is in a state that cannot be confirmed.</summary>
    public void Confirm(DateTime nowUtc)
    {
        if (Status == BookingStatus.Confirmed)
            throw new InvalidBookingStateException("Booking is already confirmed.");
        if (Status != BookingStatus.Held)
            throw new InvalidBookingStateException($"Cannot confirm a booking in state '{Status}'.");
        if (nowUtc >= ExpiresAtUtc)
            throw new InvalidBookingStateException("The hold has expired.");

        Status = BookingStatus.Confirmed;
        ConfirmedAtUtc = nowUtc;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Confirmed)
            throw new InvalidBookingStateException("A confirmed booking cannot be cancelled.");
        Status = BookingStatus.Cancelled;
    }

    /// <summary>
    /// Records that payment succeeded for this booking. Idempotent: a redelivered PaymentSucceeded
    /// event is a no-op. Only a confirmed booking can be marked paid.
    /// </summary>
    public void MarkPaid(DateTime nowUtc)
    {
        if (IsPaid) return;
        if (Status != BookingStatus.Confirmed)
            throw new InvalidBookingStateException($"Cannot mark a booking in state '{Status}' as paid.");
        PaidAtUtc = nowUtc;
    }
}
