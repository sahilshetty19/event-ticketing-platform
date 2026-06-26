namespace Catalog.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAtUtc { get; set; }

    public Guid VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}