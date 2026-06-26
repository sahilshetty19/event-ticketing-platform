namespace Booking.Domain.Exceptions;

/// <summary>Raised when an operation violates a booking invariant (e.g. confirming an expired hold).</summary>
public class InvalidBookingStateException : Exception
{
    public InvalidBookingStateException(string message) : base(message) { }
}
