using System.Net;
using System.Net.Http.Json;
using Catalog.Application.Dtos;
using Xunit;

namespace Catalog.IntegrationTests;

public class EventsEndpointsTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client;

    public EventsEndpointsTests(CatalogApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetEvents_ReturnsSeededEvent()
    {
        var events = await _client.GetFromJsonAsync<List<EventDto>>("/api/events");

        Assert.NotNull(events);
        Assert.Contains(events!, e => e.Name == "Live in Dublin" && e.City == "Dublin");
    }

    [Fact]
    public async Task GetSeats_ReturnsSeededSeats_ForExistingEvent()
    {
        var events = await _client.GetFromJsonAsync<List<EventDto>>("/api/events");
        var eventId = events!.First().Id;

        var seats = await _client.GetFromJsonAsync<List<SeatDto>>($"/api/events/{eventId}/seats");

        Assert.NotNull(seats);
        Assert.Equal(20, seats!.Count); // seeder creates 2 sections x 2 rows x 5 numbers
    }

    [Fact]
    public async Task GetEventById_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.GetAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
