using Booking.Infrastructure.Messaging;
using Booking.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Booking.IntegrationTests;

/// <summary>
/// Boots the real Booking API in-process, swapping SQL Server for the EF in-memory provider and
/// the RabbitMQ bus for MassTransit's in-memory test harness. The in-memory distributed lock is
/// selected automatically because no Redis connection string is configured.
/// </summary>
public class BookingApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // --- Database: SQL Server -> in-memory ---
            var dbDescriptors = services.Where(d =>
                d.ServiceType == typeof(BookingDbContext) ||
                (d.ServiceType.FullName?.Contains("DbContextOptions") ?? false)).ToList();
            foreach (var d in dbDescriptors)
                services.Remove(d);

            services.AddDbContext<BookingDbContext>(o =>
                o.UseInMemoryDatabase("booking-integration-tests"));

            // --- Messaging: RabbitMQ bus -> in-memory test harness ---
            var massTransitDescriptors = services.Where(d =>
                (d.ServiceType.FullName?.StartsWith("MassTransit") ?? false) ||
                (d.ImplementationType?.FullName?.StartsWith("MassTransit") ?? false)).ToList();
            foreach (var d in massTransitDescriptors)
                services.Remove(d);

            // Drop the bus hosted service so only the harness hosts a bus.
            services.RemoveAll<IHostedService>();

            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PaymentSucceededConsumer>();
            });
        });
    }
}
