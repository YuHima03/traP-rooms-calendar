namespace RoomsCalendar.Share.Domain.Repository
{
    public interface ICalendarStreamsRepository
    {
        public ValueTask<CalendarStream> GetCalendarStreamAsync(Guid streamId, CancellationToken ct = default);

        public ValueTask<CalendarStream> GetOrCreateUserCalendarStreamAsync(Guid userId, CancellationToken ct = default);

        public ValueTask<CalendarStream> RefreshCalendarStreamTokenAsync(Guid streamId, CancellationToken ct = default);
    }
}
