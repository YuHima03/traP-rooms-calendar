namespace RoomsCalendar.Share.Domain
{
    /// <summary>
    /// Represents a working room.
    /// </summary>
    public sealed record class Room(
        Guid Id,
        string PlaceName,
        DateTimeOffset AvailableSince,
        DateTimeOffset AvailableUntil
        );
}
