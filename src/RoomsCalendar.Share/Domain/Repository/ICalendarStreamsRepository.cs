namespace RoomsCalendar.Share.Domain.Repository
{
    public interface ICalendarStreamsRepository : IRepositoryBase
    {
        public ValueTask<CalendarStream?> TryGetCalendarStreamAsync(Guid streamId, CancellationToken ct = default);

        public ValueTask<CalendarStream> GetOrCreateUserCalendarStreamAsync(Guid userId, CancellationToken ct = default);

        public ValueTask<CalendarStream?> TryRefreshCalendarStreamTokenAsync(Guid streamId, CancellationToken ct = default);
    }
}
