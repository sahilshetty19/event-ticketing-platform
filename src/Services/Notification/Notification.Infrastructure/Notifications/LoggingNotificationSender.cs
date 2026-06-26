using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Notifications;

/// <summary>Stub delivery channel: logs the message instead of sending a real email/SMS.</summary>
public class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) => _logger = logger;

    public Task SendAsync(SentNotification notification, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[NOTIFICATION/{Channel}] -> customer {CustomerId}: {Message}",
            notification.Channel, notification.CustomerId, notification.Message);
        return Task.CompletedTask;
    }
}
