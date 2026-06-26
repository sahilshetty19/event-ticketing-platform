using Catalog.Application.Abstractions;
using Catalog.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService) => _eventService = eventService;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEvents(CancellationToken ct)
        => Ok(await _eventService.GetEventsAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailDto>> GetEventById(Guid id, CancellationToken ct)
    {
        var ev = await _eventService.GetEventByIdAsync(id, ct);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpGet("{id:guid}/seats")]
    [ProducesResponseType(typeof(IReadOnlyList<SeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeatDto>>> GetSeats(Guid id, CancellationToken ct)
    {
        var seats = await _eventService.GetSeatsAsync(id, ct);
        return seats is null ? NotFound() : Ok(seats);
    }
}