using Booking.Application.Abstractions;
using EventTicketing.Contracts;
using MassTransit;

namespace Booking.Infrastructure.Messaging;

/// <summary>Marks a booking as paid when its payment succeeds. Idempotent (safe under redelivery).</summary>
public class PaymentSucceededConsumer : IConsumer<PaymentSucceeded>
{
    private readonly IBookingService _bookingService;

    public PaymentSucceededConsumer(IBookingService bookingService) => _bookingService = bookingService;

    public Task Consume(ConsumeContext<PaymentSucceeded> context) =>
        _bookingService.MarkPaidAsync(context.Message.BookingId, context.CancellationToken);
}
