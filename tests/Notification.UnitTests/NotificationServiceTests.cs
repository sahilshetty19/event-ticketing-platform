using EventTicketing.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notification.Application.Abstractions;
using Notification.Application.Services;
using Notification.Domain.Entities;
using Xunit;

namespace Notification.UnitTests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<INotificationSender> _sender = new();

    private NotificationService CreateSut() =>
        new(_repo.Object, _sender.Object, NullLogger<NotificationService>.Instance);

    private static BookingConfirmed Event(Guid? bookingId = null) =>
        new(bookingId ?? Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, DateTime.UtcNow);

    [Fact]
    public async Task HandleBookingConfirmed_SendsAndStores_WhenNew()
    {
        _repo.Setup(r => r.ExistsForBookingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateSut().HandleBookingConfirmedAsync(Event());

        _sender.Verify(s => s.SendAsync(It.IsAny<SentNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<SentNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleBookingConfirmed_IsIdempotent_SkipsDuplicate()
    {
        _repo.Setup(r => r.ExistsForBookingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().HandleBookingConfirmedAsync(Event());

        _sender.Verify(s => s.SendAsync(It.IsAny<SentNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<SentNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetRecent_MapsToDtos()
    {
        var notification = SentNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "Email", "Hello", DateTime.UtcNow);
        _repo.Setup(r => r.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { notification });

        var result = await CreateSut().GetRecentAsync();

        var dto = Assert.Single(result);
        Assert.Equal(notification.BookingId, dto.BookingId);
        Assert.Equal("Email", dto.Channel);
        Assert.Equal("Hello", dto.Message);
    }
}
