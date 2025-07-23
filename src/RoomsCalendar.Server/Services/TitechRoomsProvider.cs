using RoomsCalendar.Share;
using RoomsCalendar.Share.Domain;
using System.Runtime.InteropServices;
using Yuh.Collections.Searching;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    /// <summary>
    /// Provides room information fetched from Titech open data.
    /// </summary>
    sealed class TitechRoomsProvider : IRoomsProvider
    {
        readonly List<Room> _rooms = [];

        public DateTimeOffset LastUpdatedAt { get; private set; }

        public string ProviderName => ProviderNames.Titech;

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
                LastUpdatedAt = DateTimeOffset.UtcNow;
            }
            return ValueTask.CompletedTask;
        }
    }
}
