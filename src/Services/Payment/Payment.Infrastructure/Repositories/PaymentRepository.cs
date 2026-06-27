using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _db;

    public PaymentRepository(PaymentDbContext db) => _db = db;

    public Task<Domain.Entities.Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Domain.Entities.Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default) =>
        _db.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId, ct);

    public async Task AddAsync(Domain.Entities.Payment payment, CancellationToken ct = default) =>
        await _db.Payments.AddAsync(payment, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
