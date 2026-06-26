using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.Name).IsRequired().HasMaxLength(200);
        b.Property(v => v.City).IsRequired().HasMaxLength(100);
    }
}

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).IsRequired().HasMaxLength(200);
        b.Property(e => e.Description).HasMaxLength(2000);
        b.HasOne(e => e.Venue).WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(e => e.Seats).WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.Section).IsRequired().HasMaxLength(50);
        b.Property(s => s.Row).IsRequired().HasMaxLength(10);
        b.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
    }
}