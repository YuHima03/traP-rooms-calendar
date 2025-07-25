namespace RoomsCalendar.Share.Domain.Repository
{
    public interface ICalendarStreamsRepository : IRepositoryBase
    {
        public ValueTask<CalendarStream?> TryGetCalendarStreamAsync(Guid streamId, CancellationToken ct = default);

        public ValueTask<CalendarStream> GetOrCreateUserCalendarStreamAsync(string username, CancellationToken ct = default);

        public ValueTask<CalendarStream?> TryRefreshCalendarStreamTokenAsync(Guid streamId, CancellationToken ct = default);
    }
}
