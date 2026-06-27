using Microsoft.AspNetCore.Mvc;
using Payment.Application.Abstractions;
using Payment.Application.Dtos;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    /// <summary>
    /// Processes a (mock) payment for a booking. Idempotent: repeating the same BookingId
    /// returns the original payment with 200 instead of creating a duplicate.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.ProcessAsync(request, ct);
        return result.Created
            ? CreatedAtAction(nameof(GetById), new { id = result.Payment.Id }, result.Payment)
            : Ok(result.Payment);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var payment = await _paymentService.GetByIdAsync(id, ct);
        return payment is null ? NotFound() : Ok(payment);
    }
}
