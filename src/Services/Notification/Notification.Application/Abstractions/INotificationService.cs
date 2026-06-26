using EventTicketing.Contracts;
using Notification.Application.Dtos;

namespace Notification.Application.Abstractions;

public interface INotificationService
{
    /// <summary>Handles a BookingConfirmed event idempotently (safe under at-least-once delivery).</summary>
    Task HandleBookingConfirmedAsync(BookingConfirmed message, CancellationToken ct = default);

    Task<IReadOnlyList<SentNotificationDto>> GetRecentAsync(int take = 50, CancellationToken ct = default);
}
