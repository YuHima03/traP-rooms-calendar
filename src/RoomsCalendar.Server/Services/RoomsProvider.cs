using RoomsCalendar.Share;
using RoomsCalendar.Share.Domain;
using System.Runtime.InteropServices;
using Yuh.Collections.Searching;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    sealed class RoomsProvider(string providerName) : IRoomsProvider
    {
        readonly List<Room> _rooms = [];

        public DateTimeOffset LastUpdatedAt { get; private set; }

        public required string ProviderName { get; init; } = providerName;

        public ValueTask<Room[]> GetRoomsAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken ct)
        {
            lock (_rooms)
            {
                var rooms = CollectionsMarshal.AsSpan(_rooms);
                var idxStart = BinarySearch.LowerBound<Room, DateTimeOffset>(rooms, since, RoomsExtensions.CompareAvailableUntil);
                if (idxStart == -1)
                {
                    return ValueTask.FromResult(Array.Empty<Room>());
                }
                return ValueTask.FromResult(rooms[idxStart..]
                    .AsValueEnumerable()
                    .Where(r => r.AvailableSince <= until)
                    .Order(RoomsExtensions.AvailableSinceComparer)
                    .ToArray());
            }
        }

        public ValueTask UpdateRoomsAsync<TRooms>(TRooms rooms, DateTimeOffset since, CancellationToken ct) where TRooms : IEnumerable<Room>
        {
            lock (_rooms)
            {
                var idx = BinarySearch.LowerBound<Room, DateTimeOffset>(CollectionsMarshal.AsSpan(_rooms), since, RoomsExtensions.CompareAvailableUntil);
                if (idx == 0)
                {
                    _rooms.Clear();
                }
                else if (idx > 0)
                {
                    CollectionsMarshal.SetCount(_rooms, idx);
                }

                switch (rooms)
                {
                    case Room[] array:
                        _rooms.AddRange(array.AsSpan());
                        break;
                    case List<Room> list:
                        _rooms.AddRange(CollectionsMarshal.AsSpan(list));
                        break;
                    case ArraySegment<Room> arraySegment:
                        _rooms.AddRange(arraySegment.AsSpan());
                        break;
                    default:
                        _rooms.AddRange(rooms);
                        break;
                }
                _rooms.Sort(RoomsExtensions.CompareToAvailableUntil);
                LastUpdatedAt = DateTimeOffset.UtcNow;
            }
            return ValueTask.CompletedTask;
        }
    }
}
