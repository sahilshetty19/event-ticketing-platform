using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public static class CatalogDbSeeder
{
    public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct = default)
    {
        if (await db.Venues.AnyAsync(ct)) return; // idempotent: safe to run on every startup

        var venue = new Venue { Id = Guid.NewGuid(), Name = "3Arena", City = "Dublin", Capacity = 20 };
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Live in Dublin",
            Description = "An evening of live music.",
            StartsAtUtc = DateTime.UtcNow.AddDays(30),
            VenueId = venue.Id
        };

        var seats = new List<Seat>();
        foreach (var section in new[] { "A", "B" })
            for (var row = 1; row <= 2; row++)
                for (var num = 1; num <= 5; num++)
                    seats.Add(new Seat
                    {
                        Id = Guid.NewGuid(), Section = section, Row = row.ToString(),
                        Number = num, Status = SeatStatus.Available, EventId = ev.Id
                    });

        db.Venues.Add(venue);
        db.Events.Add(ev);
        db.Seats.AddRange(seats);
        await db.SaveChangesAsync(ct);
    }
}