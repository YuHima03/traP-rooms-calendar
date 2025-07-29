namespace RoomsCalendar.Share.Domain
{
    public interface IRoomsProvider
    {
        public string ProviderName { get; }

        public DateTimeOffset LastUpdatedAt { get; }

        public ValueTask<Room[]> GetRoomsAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken ct);

        public ValueTask UpdateRoomsAsync<TRooms>(TRooms rooms, DateTimeOffset since, CancellationToken ct) where TRooms : IEnumerable<Room>;
    }
}
