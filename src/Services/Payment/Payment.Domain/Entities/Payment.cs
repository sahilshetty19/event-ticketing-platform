namespace Payment.Domain.Entities;

public enum PaymentStatus
{
    Succeeded = 0,
    Failed = 1
}

/// <summary>A processed payment for a booking. One payment per booking (BookingId is the idempotency key).</summary>
public class Payment
{
    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private Payment() { }

    /// <summary>Mock payment capture — always succeeds and assigns a fake gateway reference.</summary>
    public static Payment Capture(Guid bookingId, Guid customerId, decimal amount, DateTime nowUtc) => new()
    {
        Id = Guid.NewGuid(),
        BookingId = bookingId,
        CustomerId = customerId,
        Amount = amount,
        Status = PaymentStatus.Succeeded,
        Reference = "MOCK-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
        ProcessedAtUtc = nowUtc
    };
}
