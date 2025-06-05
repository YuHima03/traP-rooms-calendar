using RoomsCalendar.Share.Domain;

namespace RoomsCalendar.Server.Domain
{
    public interface IRoomsProvider
    {
        public ValueTask<Room[]> GetRoomsAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken ct);

        public ValueTask UpdateRoomsAsync(IEnumerable<Room> rooms, DateTimeOffset since, CancellationToken ct);
    }
}
