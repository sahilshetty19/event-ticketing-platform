namespace Notification.Application.Dtos;

public record SentNotificationDto(
    Guid Id,
    Guid BookingId,
    Guid CustomerId,
    string Channel,
    string Message,
    DateTime SentAtUtc);
