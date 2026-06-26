using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _db;

    public NotificationRepository(NotificationDbContext db) => _db = db;

    public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default) =>
        _db.SentNotifications.AnyAsync(n => n.BookingId == bookingId, ct);

    public async Task AddAsync(SentNotification notification, CancellationToken ct = default) =>
        await _db.SentNotifications.AddAsync(notification, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<SentNotification>> GetRecentAsync(int take, CancellationToken ct = default) =>
        await _db.SentNotifications
            .AsNoTracking()
            .OrderByDescending(n => n.SentAtUtc)
            .Take(take)
            .ToListAsync(ct);
}
