using Booking.Application.Abstractions;
using Booking.Application.Dtos;
using Booking.Application.Services;
using Booking.Domain.Entities;
using EventTicketing.Contracts;
using Moq;
using Xunit;

namespace Booking.UnitTests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _repo = new();
    private readonly Mock<IDistributedLock> _lock = new();
    private readonly Mock<IEventBus> _eventBus = new();

    private BookingService CreateSut() => new(_repo.Object, _lock.Object, _eventBus.Object);

    private sealed class NoopLockHandle : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private void LockGranted() =>
        _lock.Setup(l => l.AcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NoopLockHandle());

    private void LockDenied() =>
        _lock.Setup(l => l.AcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IAsyncDisposable?)null);

    private static HoldSeatRequest NewRequest(decimal amount = 75m) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), amount);

    [Fact]
    public async Task Hold_Succeeds_AndPersists_WhenSeatFreeAndLockAcquired()
    {
        LockGranted();
        _repo.Setup(r => r.GetActiveForSeatAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SeatBooking?)null);

        var result = await CreateSut().HoldAsync(NewRequest(75m));

        Assert.Equal(HoldOutcome.Success, result.Outcome);
        Assert.NotNull(result.Booking);
        Assert.Equal("Held", result.Booking!.Status);
        Assert.Equal(75m, result.Booking.Amount);
        _repo.Verify(r => r.AddAsync(It.IsAny<SeatBooking>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Hold_ReturnsSeatUnavailable_WhenActiveBookingExists()
    {
        LockGranted();
        var existing = SeatBooking.Hold(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, TimeSpan.FromMinutes(10), DateTime.UtcNow);
        _repo.Setup(r => r.GetActiveForSeatAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await CreateSut().HoldAsync(NewRequest());

        Assert.Equal(HoldOutcome.SeatUnavailable, result.Outcome);
        _repo.Verify(r => r.AddAsync(It.IsAny<SeatBooking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Hold_ReturnsLockNotAcquired_WhenLockHeldElsewhere()
    {
        LockDenied();

        var result = await CreateSut().HoldAsync(NewRequest());

        Assert.Equal(HoldOutcome.LockNotAcquired, result.Outcome);
        _repo.Verify(r => r.GetActiveForSeatAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Confirm_ReturnsNotFound_WhenBookingMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SeatBooking?)null);

        var result = await CreateSut().ConfirmAsync(Guid.NewGuid());

        Assert.Equal(ConfirmOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task Confirm_Succeeds_ForLiveHold()
    {
        var booking = SeatBooking.Hold(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, TimeSpan.FromMinutes(10), DateTime.UtcNow);
        _repo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var result = await CreateSut().ConfirmAsync(booking.Id);

        Assert.Equal(ConfirmOutcome.Success, result.Outcome);
        Assert.Equal("Confirmed", result.Booking!.Status);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<BookingConfirmed>(e => e.BookingId == booking.Id && e.Amount == booking.Amount),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Confirm_ReturnsInvalidState_ForExpiredHold()
    {
        var booking = SeatBooking.Hold(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, TimeSpan.FromMinutes(10), DateTime.UtcNow.AddMinutes(-30));
        _repo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var result = await CreateSut().ConfirmAsync(booking.Id);

        Assert.Equal(ConfirmOutcome.InvalidState, result.Outcome);
        Assert.NotNull(result.Reason);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBus.Verify(b => b.PublishAsync(It.IsAny<BookingConfirmed>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
