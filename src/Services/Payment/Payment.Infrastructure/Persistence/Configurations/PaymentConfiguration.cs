using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payment.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Payment> b)
    {
        b.ToTable("Payments");
        b.HasKey(x => x.Id);

        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Reference).IsRequired().HasMaxLength(50);

        // One payment per booking — enforces idempotency at the DB level.
        b.HasIndex(x => x.BookingId).IsUnique();
    }
}
