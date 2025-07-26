using RoomsCalendar.Share;
using RoomsCalendar.Share.Domain;
using System.Runtime.CompilerServices;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    sealed class RoomsCalendarProvider(
        [FromKeyedServices(ProviderNames.Knoq)] IRoomsProvider roomsProvider,
        TimeZoneInfo? calendarTimeZone = null)
    {
        Ical.Net.Serialization.CalendarSerializer CalendarSerializer { get; init; } = new();

        DateTimeOffset LastUpdatedAt { get; set; }

        readonly SemaphoreSlim _lock = new(1, 1);

        string? _cachedAll = null;
        string? _cachedExcludeOccupied = null;

        public async ValueTask<string> GetIcalStringAsync(bool excludeOccupied, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (excludeOccupied)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (_cachedAll is null || LastUpdatedAt < roomsProvider.LastUpdatedAt)
                    {
                        var rooms = await GetRoomsAsync(ct).ConfigureAwait(false);
                        _cachedAll = GetIcalStringCore(rooms);
                        LastUpdatedAt = roomsProvider.LastUpdatedAt;
                        return _cachedAll;
                    }
                    else
                    {
                        return _cachedAll;
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        string GetIcalStringCore(IEnumerable<Room> rooms)
        {
            var timeZoneInfo = calendarTimeZone ?? TimeZoneInfo.Utc;
            Ical.Net.Calendar calendar = new()
            {
                ProductId = "-//yuhima03//traP Rooms Calendar v1//JA",
                Version = "2.0"
            };
            calendar.AddTimeZone(timeZoneInfo);
            calendar.Events.AddRange(rooms.Select(r => new Ical.Net.CalendarComponents.CalendarEvent
            {
                DtStart = new(TimeZoneInfo.ConvertTimeFromUtc(r.AvailableSince.UtcDateTime, timeZoneInfo)),
                DtEnd = new(TimeZoneInfo.ConvertTimeFromUtc(r.AvailableUntil.UtcDateTime, timeZoneInfo)),
                Location = r.PlaceName,
                Summary = $"進捗部屋 {GetSimplePlaceName(r.PlaceName)}",
                Uid = GetRoomUid(r),
            }));
            lock (CalendarSerializer)
            {
                return CalendarSerializer.SerializeToString(calendar) ?? string.Empty;
            }
        }

        ValueTask<Room[]> GetRoomsAsync(CancellationToken ct = default)
        {
            var timeZoneInfo = calendarTimeZone ?? TimeZoneInfo.Utc;
            DateTimeOffset timeFrom = TimeZoneInfo.ConvertTimeToUtc(
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo).Date.AddDays(-14),
                timeZoneInfo
            );
            return roomsProvider.GetRoomsAsync(timeFrom, DateTimeOffset.MaxValue, ct);
        }

        static ReadOnlySpan<char> GetSimplePlaceName(ReadOnlySpan<char> fullPlaceName)
        {
            var idxSpace = fullPlaceName.IndexOf('\x20');
            return (idxSpace == -1) ? fullPlaceName : fullPlaceName[..idxSpace];
        }

        static string GetRoomUid(Room room)
        {
            DefaultInterpolatedStringHandler str = $"{room.AvailableSince.UtcDateTime:yyyyMMdd'T'HHmmss}R";
            str.AppendFormatted(GetSimplePlaceName(room.PlaceName));
            return str.ToStringAndClear();
        }
    }
}
