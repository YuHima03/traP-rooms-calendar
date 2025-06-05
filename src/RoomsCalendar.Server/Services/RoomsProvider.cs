using RoomsCalendar.Share.Domain;
using System.Runtime.InteropServices;
using Yuh.Collections.Searching;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    sealed class RoomsProvider : IRoomsProvider
    {
        readonly List<Room> _rooms = [];

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

        public ValueTask UpdateRoomsAsync(IEnumerable<Room> rooms, DateTimeOffset since, CancellationToken ct)
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
                _rooms.AddRange(rooms);
                _rooms.Sort(RoomsExtensions.CompareToAvailableUntil);
            }
            return ValueTask.CompletedTask;
        }
    }

    static class RoomsExtensions
    {
        public static readonly Comparer<Room> AvailableSinceComparer = Comparer<Room>.Create(CompareToAvailableSince);

        public static int CompareToAvailableSince(this Room room, Room other)
        {
            return room.AvailableSince.CompareTo(other.AvailableSince);
        }

        public static int CompareToAvailableUntil(this Room room, Room other)
        {
            return room.AvailableUntil.CompareTo(other.AvailableUntil);
        }

        public static int CompareAvailableUntil(this Room room, DateTimeOffset since)
        {
            return room.AvailableUntil.CompareTo(since);
        }

        public static Room ToRoom(this Knoq.Model.ResponseRoom room)
        {
            return new Room(
                room.Place,
                DateTimeOffset.Parse(room.TimeStart),
                DateTimeOffset.Parse(room.TimeEnd)
            );
        }
    }
}
