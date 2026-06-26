using Notification.Domain.Entities;

namespace Notification.Application.Abstractions;

public interface INotificationRepository
{
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default);
    Task AddAsync(SentNotification notification, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SentNotification>> GetRecentAsync(int take, CancellationToken ct = default);
}
