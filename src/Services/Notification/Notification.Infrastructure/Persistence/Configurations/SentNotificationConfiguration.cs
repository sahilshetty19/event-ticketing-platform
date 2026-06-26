using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public class SentNotificationConfiguration : IEntityTypeConfiguration<SentNotification>
{
    public void Configure(EntityTypeBuilder<SentNotification> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Channel).IsRequired().HasMaxLength(50);
        b.Property(x => x.Message).IsRequired().HasMaxLength(1000);

        // One notification per booking — the unique index enforces idempotency at the DB level.
        b.HasIndex(x => x.BookingId).IsUnique();
    }
}
