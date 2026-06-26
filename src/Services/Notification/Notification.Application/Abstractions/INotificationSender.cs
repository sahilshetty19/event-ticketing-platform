using Notification.Domain.Entities;

namespace Notification.Application.Abstractions;

/// <summary>Delivery channel port. The stub implementation just logs; a real one would call email/SMS.</summary>
public interface INotificationSender
{
    Task SendAsync(SentNotification notification, CancellationToken ct = default);
}
