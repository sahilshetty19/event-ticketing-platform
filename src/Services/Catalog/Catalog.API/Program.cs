using Catalog.Application;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Migrate + seed on startup, retrying so the app waits for the database to accept connections
// instead of crashing if it starts before SQL Server is ready.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            // Relational providers (SQL Server) run migrations; the in-memory provider used by
            // integration tests has no migration support, so just ensure the schema exists.
            if (db.Database.IsRelational())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();
            break;
        }
        catch (Exception ex) when (attempt < 12)
        {
            logger.LogWarning(ex, "Database not ready (attempt {Attempt}/12); retrying in 5s...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
    await CatalogDbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

// Exposed so integration tests can spin up the app via WebApplicationFactory.
public partial class Program { }
