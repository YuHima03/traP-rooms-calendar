namespace RoomsCalendar.Share.Domain
{
    public record class CalendarStream(
        Guid Id,
        string Username,
        string Token,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
        );
}
