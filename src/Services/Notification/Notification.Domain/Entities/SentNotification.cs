namespace Notification.Domain.Entities;

/// <summary>
/// Record of a notification we have already delivered. Doubles as the idempotency ledger:
/// one row per BookingId means a redelivered event is recognised and skipped.
/// </summary>
public class SentNotification
{
    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Channel { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTime SentAtUtc { get; private set; }

    private SentNotification() { }

    public static SentNotification Create(
        Guid bookingId, Guid customerId, string channel, string message, DateTime sentAtUtc) => new()
    {
        Id = Guid.NewGuid(),
        BookingId = bookingId,
        CustomerId = customerId,
        Channel = channel,
        Message = message,
        SentAtUtc = sentAtUtc
    };
}
