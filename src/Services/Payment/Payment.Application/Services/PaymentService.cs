using EventTicketing.Contracts;
using Payment.Application.Abstractions;
using Payment.Application.Dtos;

namespace Payment.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IEventBus _eventBus;

    public PaymentService(IPaymentRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<PaymentResult> ProcessAsync(ProcessPaymentRequest request, CancellationToken ct = default)
    {
        // Idempotency: one payment per booking. A replay (retried HTTP call or redelivered event)
        // returns the existing payment and does NOT charge or publish again.
        var existing = await _repository.GetByBookingIdAsync(request.BookingId, ct);
        if (existing is not null)
            return new PaymentResult(ToResponse(existing), Created: false);

        var payment = Domain.Entities.Payment.Capture(
            request.BookingId, request.CustomerId, request.Amount, DateTime.UtcNow);

        await _repository.AddAsync(payment, ct);
        await _repository.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(new PaymentSucceeded(
            payment.Id, payment.BookingId, payment.CustomerId, payment.Amount, payment.ProcessedAtUtc), ct);

        return new PaymentResult(ToResponse(payment), Created: true);
    }

    public async Task<PaymentResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await _repository.GetByIdAsync(id, ct);
        return payment is null ? null : ToResponse(payment);
    }

    private static PaymentResponse ToResponse(Domain.Entities.Payment p) => new(
        p.Id, p.BookingId, p.CustomerId, p.Amount, p.Status.ToString(), p.Reference, p.ProcessedAtUtc);
}
