using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<SeatBooking> Bookings => Set<SeatBooking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
}
