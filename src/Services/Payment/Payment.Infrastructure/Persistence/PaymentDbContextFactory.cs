using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payment.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef` so tooling needs no live DB or committed secret.</summary>
public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__PaymentDb")
            ?? "Server=localhost;Database=PaymentDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new PaymentDbContext(options);
    }
}
