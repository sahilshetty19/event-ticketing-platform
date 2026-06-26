namespace Catalog.Application.Dtos;

public record EventDto(Guid Id, string Name, string Description,
    DateTime StartsAtUtc, string VenueName, string City);

public record EventDetailDto(Guid Id, string Name, string Description,
    DateTime StartsAtUtc, string VenueName, string City, int SeatCount);

public record SeatDto(Guid Id, string Section, string Row, int Number, string Status);