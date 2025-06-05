using RoomsCalendar.Server.Domain;
using RoomsCalendar.Share.Domain;
using System.Runtime.InteropServices;
using Yuh.Collections.Searching;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    sealed class RoomsProvider : IRoomsProvider
    {
        readonly RoomsList _rooms = new();

        public ValueTask<Room[]> GetRoomsAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken ct)
        {
            lock (_rooms)
            {
                return new ValueTask<Room[]>(_rooms.Get(since, until));
            }
        }

        public ValueTask UpdateRoomsAsync(IEnumerable<Room> rooms, DateTimeOffset since, CancellationToken ct)
        {
            lock(_rooms)
            {
                _rooms.Update(rooms, since);
            }
            return ValueTask.CompletedTask;
        }
    }

    sealed class RoomsList
    {
        /// <summary>
        /// <see cref="Room.AvailableUntil"/> の昇順でソートされている.
        /// </summary>
        /// <remarks>
        /// 検索は基本的に未来の情報の取得に使われることが想定されるため, <see cref="Room.AvailableUntil"/> との比較で絞り込むことで計算量の削減が見込まれる.
        /// </remarks>
        readonly List<Room> _items = [];

        readonly Dictionary<Guid, int> _index = [];

        public Room[] Get(DateTimeOffset since, DateTimeOffset until)
        {
            var items = CollectionsMarshal.AsSpan(_items);
            var idxStart = BinarySearch.LowerBound<Room, DateTimeOffset>(items, since, RoomsExtensions.CompareAvailableUntil);
            if (idxStart == -1)
            {
                return [];
            }
            return items[idxStart..]
                .AsValueEnumerable()
                .Where(r => r.AvailableSince <= until)
                .Order(RoomsExtensions.AvailableSinceComparer)
                .ToArray();
        }

        public void Update(IEnumerable<Room> rooms, DateTimeOffset since)
        {
            var idx = BinarySearch.LowerBound<Room, DateTimeOffset>(CollectionsMarshal.AsSpan(_items), since, RoomsExtensions.CompareAvailableUntil);
            if (idx == 0)
            {
                _items.Clear();
            }
            else if (idx > 0)
            {
                CollectionsMarshal.SetCount(_items, idx);
            }

            foreach (var r in rooms)
            {
                if (_index.Remove(r.Id, out var i))
                {
                    _items[i] = r;
                }
                else
                {
                    _items.Add(r);
                }
            }

            _items.Sort(RoomsExtensions.CompareToAvailableUntil);
            var items = CollectionsMarshal.AsSpan(_items);
            for (var i = 0; i < items.Length; i++)
            {
                _index[items[i].Id] = i;
            }
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
                room.RoomId,
                room.Place,
                DateTimeOffset.Parse(room.TimeStart),
                DateTimeOffset.Parse(room.TimeEnd)
            );
        }
    }
}
