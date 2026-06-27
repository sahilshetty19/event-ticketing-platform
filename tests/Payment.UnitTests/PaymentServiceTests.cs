using EventTicketing.Contracts;
using Moq;
using Payment.Application.Abstractions;
using Payment.Application.Dtos;
using Payment.Application.Services;
using Xunit;

namespace Payment.UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();

    private PaymentService CreateSut() => new(_repo.Object, _eventBus.Object);

    private static ProcessPaymentRequest NewRequest(decimal amount = 50m) =>
        new(Guid.NewGuid(), Guid.NewGuid(), amount);

    [Fact]
    public async Task Process_CreatesPayment_AndPublishes_WhenFirstTime()
    {
        _repo.Setup(r => r.GetByBookingIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment.Domain.Entities.Payment?)null);
        var request = NewRequest(120m);

        var result = await CreateSut().ProcessAsync(request);

        Assert.True(result.Created);
        Assert.Equal("Succeeded", result.Payment.Status);
        Assert.Equal(120m, result.Payment.Amount);
        Assert.StartsWith("MOCK-", result.Payment.Reference);
        _repo.Verify(r => r.AddAsync(It.IsAny<Payment.Domain.Entities.Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<PaymentSucceeded>(e => e.BookingId == request.BookingId && e.Amount == 120m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Process_IsIdempotent_ReturnsExisting_AndDoesNotPublishAgain()
    {
        var request = NewRequest(80m);
        var existing = Payment.Domain.Entities.Payment.Capture(
            request.BookingId, request.CustomerId, 80m, DateTime.UtcNow);
        _repo.Setup(r => r.GetByBookingIdAsync(request.BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await CreateSut().ProcessAsync(request);

        Assert.False(result.Created);
        Assert.Equal(existing.Id, result.Payment.Id);
        _repo.Verify(r => r.AddAsync(It.IsAny<Payment.Domain.Entities.Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventBus.Verify(b => b.PublishAsync(It.IsAny<PaymentSucceeded>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment.Domain.Entities.Payment?)null);

        Assert.Null(await CreateSut().GetByIdAsync(Guid.NewGuid()));
    }
}
