using EventTicketing.Contracts;
using MassTransit;
using Notification.Application.Abstractions;

namespace Notification.Infrastructure.Messaging;

/// <summary>Consumes BookingConfirmed and delegates to the (idempotent) notification service.</summary>
public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
{
    private readonly INotificationService _notificationService;

    public BookingConfirmedConsumer(INotificationService notificationService) =>
        _notificationService = notificationService;

    public Task Consume(ConsumeContext<BookingConfirmed> context) =>
        _notificationService.HandleBookingConfirmedAsync(context.Message, context.CancellationToken);
}
