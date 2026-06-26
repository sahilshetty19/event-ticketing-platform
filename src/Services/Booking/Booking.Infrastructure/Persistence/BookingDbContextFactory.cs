using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Booking.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef` so tooling needs no live DB or committed secret.</summary>
public class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BookingDb")
            ?? "Server=localhost;Database=BookingDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BookingDbContext(options);
    }
}
