using Booking.Application.Abstractions;
using Booking.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

    /// <summary>Places a time-boxed hold on a seat.</summary>
    [HttpPost("hold")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Hold([FromBody] HoldSeatRequest request, CancellationToken ct)
    {
        var result = await _bookingService.HoldAsync(request, ct);
        return result.Outcome switch
        {
            HoldOutcome.Success =>
                CreatedAtAction(nameof(GetById), new { id = result.Booking!.Id }, result.Booking),
            HoldOutcome.SeatUnavailable =>
                Conflict(new { error = "Seat is already held or booked." }),
            HoldOutcome.LockNotAcquired =>
                Conflict(new { error = "Seat is currently being processed; please retry." }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>Confirms a live hold, making the booking final.</summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _bookingService.ConfirmAsync(id, ct);
        return result.Outcome switch
        {
            ConfirmOutcome.Success => Ok(result.Booking),
            ConfirmOutcome.NotFound => NotFound(),
            ConfirmOutcome.InvalidState => Conflict(new { error = result.Reason }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.GetByIdAsync(id, ct);
        return booking is null ? NotFound() : Ok(booking);
    }
}
