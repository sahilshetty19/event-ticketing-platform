using Booking.Domain.Entities;
using Booking.Domain.Exceptions;
using Xunit;

namespace Booking.UnitTests;

public class SeatBookingTests
{
    private static readonly Guid EventId = Guid.NewGuid();
    private static readonly Guid SeatId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    [Fact]
    public void Hold_CreatesHeldBooking_WithExpiryInTheFuture()
    {
        var now = DateTime.UtcNow;

        var booking = SeatBooking.Hold(EventId, SeatId, CustomerId, 50m, TimeSpan.FromMinutes(10), now);

        Assert.Equal(BookingStatus.Held, booking.Status);
        Assert.Equal(now.AddMinutes(10), booking.ExpiresAtUtc);
        Assert.Null(booking.ConfirmedAtUtc);
    }

    [Fact]
    public void Hold_Throws_WhenAmountNegative()
    {
        Assert.Throws<InvalidBookingStateException>(() =>
            SeatBooking.Hold(EventId, SeatId, CustomerId, -1m, TimeSpan.FromMinutes(10), DateTime.UtcNow));
    }

    [Fact]
    public void Confirm_TransitionsToConfirmed()
    {
        var now = DateTime.UtcNow;
        var booking = SeatBooking.Hold(EventId, SeatId, CustomerId, 50m, TimeSpan.FromMinutes(10), now);

        booking.Confirm(now.AddMinutes(1));

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_Throws_WhenHoldExpired()
    {
        var heldAt = DateTime.UtcNow.AddMinutes(-20);
        var booking = SeatBooking.Hold(EventId, SeatId, CustomerId, 50m, TimeSpan.FromMinutes(10), heldAt);

        var ex = Assert.Throws<InvalidBookingStateException>(() => booking.Confirm(DateTime.UtcNow));
        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Confirm_Throws_WhenAlreadyConfirmed()
    {
        var now = DateTime.UtcNow;
        var booking = SeatBooking.Hold(EventId, SeatId, CustomerId, 50m, TimeSpan.FromMinutes(10), now);
        booking.Confirm(now);

        Assert.Throws<InvalidBookingStateException>(() => booking.Confirm(now));
    }

    [Fact]
    public void IsExpired_IsTrue_OnlyWhenHeldPastExpiry()
    {
        var heldAt = DateTime.UtcNow.AddMinutes(-20);
        var booking = SeatBooking.Hold(EventId, SeatId, CustomerId, 50m, TimeSpan.FromMinutes(10), heldAt);

        Assert.True(booking.IsExpired(DateTime.UtcNow));
        Assert.False(booking.IsExpired(heldAt.AddMinutes(5)));
    }
}
