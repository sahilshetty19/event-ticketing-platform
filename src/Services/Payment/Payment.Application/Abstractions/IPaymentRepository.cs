using Payment.Domain.Entities;

namespace Payment.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Domain.Entities.Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Domain.Entities.Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task AddAsync(Domain.Entities.Payment payment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
