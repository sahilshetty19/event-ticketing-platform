using Booking.Application.Dtos;

namespace Booking.Application.Abstractions;

public interface IBookingService
{
    Task<HoldResult> HoldAsync(HoldSeatRequest request, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmAsync(Guid bookingId, CancellationToken ct = default);
    Task<BookingResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Marks a confirmed booking as paid in response to PaymentSucceeded. Idempotent.</summary>
    Task MarkPaidAsync(Guid bookingId, CancellationToken ct = default);
}
