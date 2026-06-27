using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Booking.Infrastructure.Persistence;

// SQL Server has no timezone info, so EF reads DateTime values back as Kind=Unspecified.
// These converters re-tag reads as UTC so JSON serialization emits the 'Z' designator and
// clients parse the instants as UTC rather than local time.
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
}

public sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
}
