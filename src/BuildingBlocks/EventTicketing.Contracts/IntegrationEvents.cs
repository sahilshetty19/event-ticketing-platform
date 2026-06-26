namespace EventTicketing.Contracts;

/// <summary>Published by Booking when a held seat is confirmed. Consumed by Payment and Notification.</summary>
public record BookingConfirmed(
    Guid BookingId,
    Guid EventId,
    Guid SeatId,
    Guid CustomerId,
    decimal Amount,
    DateTime ConfirmedAtUtc);

/// <summary>Published by Payment once a booking's payment succeeds. Consumed by Notification (and Booking).</summary>
public record PaymentSucceeded(
    Guid PaymentId,
    Guid BookingId,
    Guid CustomerId,
    decimal Amount,
    DateTime ProcessedAtUtc);
