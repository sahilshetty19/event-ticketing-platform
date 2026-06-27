using EventTicketing.Contracts;
using MassTransit;
using Payment.Application.Abstractions;
using Payment.Application.Dtos;

namespace Payment.Infrastructure.Messaging;

/// <summary>
/// Consumes BookingConfirmed and charges the booking via the (idempotent) payment service,
/// which in turn publishes PaymentSucceeded. Safe under at-least-once delivery.
/// </summary>
public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
{
    private readonly IPaymentService _paymentService;

    public BookingConfirmedConsumer(IPaymentService paymentService) => _paymentService = paymentService;

    public Task Consume(ConsumeContext<BookingConfirmed> context)
    {
        var message = context.Message;
        return _paymentService.ProcessAsync(
            new ProcessPaymentRequest(message.BookingId, message.CustomerId, message.Amount),
            context.CancellationToken);
    }
}
