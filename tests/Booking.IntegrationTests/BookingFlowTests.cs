using System.Net;
using System.Net.Http.Json;
using Booking.Application.Dtos;
using EventTicketing.Contracts;
using MassTransit;
using MassTransit.Testing;
using Xunit;

namespace Booking.IntegrationTests;

public class BookingFlowTests : IClassFixture<BookingApiFactory>
{
    private readonly BookingApiFactory _factory;

    public BookingFlowTests(BookingApiFactory factory) => _factory = factory;

    private static object NewHoldRequest() => new
    {
        eventId = Guid.NewGuid(),
        seatId = Guid.NewGuid(),
        customerId = Guid.NewGuid(),
        amount = 99m
    };

    [Fact]
    public async Task Hold_Then_Confirm_Publishes_BookingConfirmed()
    {
        var client = _factory.CreateClient();              // starts the host (and the test harness)
        var harness = _factory.Services.GetTestHarness();

        // Hold
        var holdResponse = await client.PostAsJsonAsync("/api/bookings/hold", NewHoldRequest());
        Assert.Equal(HttpStatusCode.Created, holdResponse.StatusCode);
        var booking = await holdResponse.Content.ReadFromJsonAsync<BookingResponse>();
        Assert.NotNull(booking);

        // Confirm
        var confirmResponse = await client.PostAsync($"/api/bookings/{booking!.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        // The integration event must have been published to the bus.
        Assert.True(await harness.Published.Any<BookingConfirmed>(
            x => x.Context.Message.BookingId == booking.Id));
    }

    [Fact]
    public async Task Double_Hold_For_Same_Seat_Is_Rejected()
    {
        var client = _factory.CreateClient();
        var request = NewHoldRequest();

        var first = await client.PostAsJsonAsync("/api/bookings/hold", request);
        var second = await client.PostAsJsonAsync("/api/bookings/hold", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode); // seat already held
    }

    [Fact]
    public async Task PaymentSucceeded_Marks_Booking_Paid()
    {
        var client = _factory.CreateClient();
        var harness = _factory.Services.GetTestHarness();

        // Arrange: a confirmed booking.
        var holdResponse = await client.PostAsJsonAsync("/api/bookings/hold", NewHoldRequest());
        var booking = await holdResponse.Content.ReadFromJsonAsync<BookingResponse>();
        await client.PostAsync($"/api/bookings/{booking!.Id}/confirm", null);

        // Act: payment succeeds elsewhere -> Booking consumes the event.
        await harness.Bus.Publish(new PaymentSucceeded(
            Guid.NewGuid(), booking.Id, booking.CustomerId, booking.Amount, DateTime.UtcNow));

        Assert.True(await harness.Consumed.Any<PaymentSucceeded>());

        // Assert: the booking is now marked paid.
        var refreshed = await client.GetFromJsonAsync<BookingResponse>($"/api/bookings/{booking.Id}");
        Assert.NotNull(refreshed);
        Assert.NotNull(refreshed!.PaidAtUtc);
    }
}
