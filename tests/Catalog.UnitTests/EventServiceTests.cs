using Catalog.Application.Abstractions;
using Catalog.Application.Dtos;
using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Moq;
using Xunit;

namespace Catalog.UnitTests;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _repo = new();
    private readonly Mock<ICacheService> _cache = new();

    private EventService CreateSut() => new(_repo.Object, _cache.Object);

    /// <summary>Configures the cache to always miss for type <typeparamref name="T"/> (invokes the factory).</summary>
    private void CacheAlwaysMiss<T>() where T : class =>
        _cache.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<T?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string _, Func<CancellationToken, Task<T?>> factory, TimeSpan? _, CancellationToken ct) => factory(ct));

    private static Event SampleEvent(int seatCount = 0)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Live in Dublin",
            Description = "An evening of live music.",
            StartsAtUtc = new DateTime(2026, 9, 1, 19, 0, 0, DateTimeKind.Utc),
            Venue = new Venue { Id = Guid.NewGuid(), Name = "3Arena", City = "Dublin" }
        };
        for (var i = 0; i < seatCount; i++)
            ev.Seats.Add(new Seat { Id = Guid.NewGuid(), Section = "A", Row = "1", Number = i + 1 });
        return ev;
    }

    [Fact]
    public async Task GetEventsAsync_MapsEntitiesToDtos_OnCacheMiss()
    {
        CacheAlwaysMiss<List<EventDto>>();
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { SampleEvent() });

        var result = await CreateSut().GetEventsAsync();

        var dto = Assert.Single(result);
        Assert.Equal("Live in Dublin", dto.Name);
        Assert.Equal("3Arena", dto.VenueName);
        Assert.Equal("Dublin", dto.City);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsCachedValue_WithoutHittingRepository_OnCacheHit()
    {
        var cached = new List<EventDto>
        {
            new(Guid.NewGuid(), "Cached", "", DateTime.UtcNow, "Venue", "City")
        };
        _cache.Setup(c => c.GetOrSetAsync(
                "catalog:events:all",
                It.IsAny<Func<CancellationToken, Task<List<EventDto>?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await CreateSut().GetEventsAsync();

        Assert.Same(cached[0].Name, Assert.Single(result).Name);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsNull_WhenEventNotFound()
    {
        CacheAlwaysMiss<EventDetailDto>();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await CreateSut().GetEventByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEventByIdAsync_ProjectsSeatCount()
    {
        CacheAlwaysMiss<EventDetailDto>();
        var ev = SampleEvent(seatCount: 3);
        _repo.Setup(r => r.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);

        var result = await CreateSut().GetEventByIdAsync(ev.Id);

        Assert.NotNull(result);
        Assert.Equal(3, result!.SeatCount);
        Assert.Equal("3Arena", result.VenueName);
    }

    [Fact]
    public async Task GetSeatsAsync_ReturnsNull_AndSkipsSeatLookup_WhenEventMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await CreateSut().GetSeatsAsync(Guid.NewGuid());

        Assert.Null(result);
        _repo.Verify(r => r.GetSeatsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSeatsAsync_ReturnsMappedSeats_WhenEventExists()
    {
        var ev = SampleEvent();
        _repo.Setup(r => r.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);
        CacheAlwaysMiss<List<SeatDto>>();
        _repo.Setup(r => r.GetSeatsAsync(ev.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Seat { Id = Guid.NewGuid(), Section = "A", Row = "1", Number = 1, Status = SeatStatus.Available },
                new Seat { Id = Guid.NewGuid(), Section = "A", Row = "1", Number = 2, Status = SeatStatus.Booked }
            });

        var result = await CreateSut().GetSeatsAsync(ev.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("Available", result[0].Status);
        Assert.Equal("Booked", result[1].Status);
    }
}
