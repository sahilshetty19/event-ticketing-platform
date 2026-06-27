namespace Payment.Application.Dtos;

public record ProcessPaymentRequest(Guid BookingId, Guid CustomerId, decimal Amount);

public record PaymentResponse(
    Guid Id,
    Guid BookingId,
    Guid CustomerId,
    decimal Amount,
    string Status,
    string Reference,
    DateTime ProcessedAtUtc);

/// <summary><see cref="Created"/> is false when an existing payment was returned (idempotent replay).</summary>
public record PaymentResult(PaymentResponse Payment, bool Created);
