namespace Catalog.Domain.Entities;

public enum SeatStatus
{
    Available = 0,
    Held = 1,
    Booked = 2
}

public class Seat
{
    public Guid Id { get; set; }
    public string Section { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public SeatStatus Status { get; set; } = SeatStatus.Available;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
}