namespace RoomsCalendar.Share.Domain
{
    public record class CalendarStream(
        Guid Id,
        Guid UserId,
        string Token,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
        );
}
