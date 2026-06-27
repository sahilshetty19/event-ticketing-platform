namespace Booking.Application.Dtos;

public record HoldSeatRequest(Guid EventId, Guid SeatId, Guid CustomerId, decimal Amount);

public record BookingResponse(
    Guid Id,
    Guid EventId,
    Guid SeatId,
    Guid CustomerId,
    decimal Amount,
    string Status,
    DateTime HeldAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? PaidAtUtc);

public enum HoldOutcome
{
    Success,
    SeatUnavailable,
    LockNotAcquired
}

public record HoldResult(HoldOutcome Outcome, BookingResponse? Booking);

public enum ConfirmOutcome
{
    Success,
    NotFound,
    InvalidState
}

public record ConfirmResult(ConfirmOutcome Outcome, BookingResponse? Booking, string? Reason);
