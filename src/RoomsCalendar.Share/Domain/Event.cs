namespace RoomsCalendar.Share.Domain
{
    public sealed record class Event(
        Guid Id,
        string Name,
        string PlaceName,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        bool OccupiesRoom
        );
}
