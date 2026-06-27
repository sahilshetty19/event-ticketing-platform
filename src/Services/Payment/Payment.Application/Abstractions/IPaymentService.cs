using Payment.Application.Dtos;

namespace Payment.Application.Abstractions;

public interface IPaymentService
{
    /// <summary>Processes a payment idempotently (keyed by BookingId); replays return the existing payment.</summary>
    Task<PaymentResult> ProcessAsync(ProcessPaymentRequest request, CancellationToken ct = default);

    Task<PaymentResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
