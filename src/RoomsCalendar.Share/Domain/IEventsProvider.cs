namespace RoomsCalendar.Share.Domain
{
    public interface IEventsProvider
    {
        public DateTimeOffset LastUpdatedAt { get; }

        public ValueTask<Event[]> GetEventsAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken ct);

        public ValueTask UpdateEventsAsync(IEnumerable<Event> events, DateTimeOffset since, CancellationToken ct);
    }
}
