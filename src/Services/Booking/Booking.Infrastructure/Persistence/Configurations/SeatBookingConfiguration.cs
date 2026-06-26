using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Booking.Infrastructure.Persistence.Configurations;

public class SeatBookingConfiguration : IEntityTypeConfiguration<SeatBooking>
{
    public void Configure(EntityTypeBuilder<SeatBooking> b)
    {
        b.ToTable("Bookings");
        b.HasKey(x => x.Id);

        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");

        // Lookup index for the active-hold check.
        b.HasIndex(x => new { x.EventId, x.SeatId });

        // DB-level guarantee (defence in depth alongside the distributed lock):
        // a seat can be confirmed at most once.
        b.HasIndex(x => new { x.EventId, x.SeatId })
            .IsUnique()
            .HasFilter("[Status] = 'Confirmed'")
            .HasDatabaseName("UX_Bookings_Event_Seat_Confirmed");
    }
}
