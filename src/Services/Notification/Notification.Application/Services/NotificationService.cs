using EventTicketing.Contracts;
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Dtos;
using Notification.Domain.Entities;

namespace Notification.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationSender _sender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        INotificationSender sender,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _sender = sender;
        _logger = logger;
    }

    public async Task HandleBookingConfirmedAsync(BookingConfirmed message, CancellationToken ct = default)
    {
        // At-least-once delivery: the same event may arrive more than once. The BookingId acts as
        // the idempotency key, so a duplicate is a no-op rather than a second notification.
        if (await _repository.ExistsForBookingAsync(message.BookingId, ct))
        {
            _logger.LogInformation(
                "Duplicate BookingConfirmed for booking {BookingId}; notification already sent.",
                message.BookingId);
            return;
        }

        var body =
            $"Hi! Your booking {message.BookingId} for seat {message.SeatId} at event {message.EventId} " +
            $"is confirmed. Amount charged: {message.Amount:0.00}.";

        var notification = SentNotification.Create(
            message.BookingId, message.CustomerId, channel: "Email", message: body, sentAtUtc: DateTime.UtcNow);

        await _sender.SendAsync(notification, ct);
        await _repository.AddAsync(notification, ct);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SentNotificationDto>> GetRecentAsync(int take = 50, CancellationToken ct = default)
    {
        var items = await _repository.GetRecentAsync(take, ct);
        return items
            .Select(n => new SentNotificationDto(n.Id, n.BookingId, n.CustomerId, n.Channel, n.Message, n.SentAtUtc))
            .ToList();
    }
}
