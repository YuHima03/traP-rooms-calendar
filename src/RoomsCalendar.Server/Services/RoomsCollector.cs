using Knoq;
using RoomsCalendar.Server.Domain;
using RoomsCalendar.Share.Domain;

namespace RoomsCalendar.Server.Services
{
    sealed class RoomsCollector(
        IKnoqApiClient knoq,
        ILogger<RoomsCollector> logger,
        IRoomsProvider roomsProvider,
        TimeZoneInfo? timeZoneInfo = null
        ) : BackgroundService
    {
        readonly TimeZoneInfo _timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            PeriodicTimer timer = new(TimeSpan.FromSeconds(15));
            DateTimeOffset lastFullCollection = DateTimeOffset.MinValue;
            do
            {
                try
                {
                    var utcNow = DateTimeOffset.UtcNow;
                    List<Knoq.Model.ResponseRoom> rooms;

                    DateTimeOffset since = DateTimeOffset.MinValue;
                    if (utcNow - lastFullCollection < TimeSpan.FromDays(1))
                    {
                        since = TimeZoneInfo.ConvertTimeToUtc(
                            TimeZoneInfo.ConvertTimeFromUtc(utcNow.UtcDateTime, _timeZoneInfo).Date,
                            _timeZoneInfo
                        );
                    }
                    else
                    {
                        lastFullCollection = utcNow;
                    }

                        rooms = await knoq.RoomsApi.GetRoomsAsync(dateBegin: since.ToString("O"), cancellationToken: stoppingToken);

                    var filterd = rooms
                        .Where(r => r.Verified)
                        .Select(InternalExtensions.KnoqResponseToRoom)
                        .GroupBy(r => r.Place)
                        .Select(g => g
                            .OrderBy(r => r.StartsAt)
                            .DistinctByTime())
                        .SelectMany(g => g
                            .UnionContiguous()
                            .Select(InternalExtensions.KnoqRoomToDomainRoom));

                    await roomsProvider.UpdateRoomsAsync(filterd, since, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while collecting rooms.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }

    file sealed record class KnoqRoom(
        Guid Id,
        string Place,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        DateTimeOffset UpdatedAt
        );

    file static class InternalExtensions
    {
        /// <summary>
        /// 重複部分を除去する.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<KnoqRoom> DistinctByTime(this IEnumerable<KnoqRoom> enumerable)
        {
            using var en = enumerable.GetEnumerator();
            if (!en.MoveNext())
            {
                yield break;
            }
            var accumulated = en.Current;
            while (en.MoveNext())
            {
                // 重複部分がある場合は, 最新のものを優先する.
                // 更新時刻が同一の場合は, 区間の和を取る.
                var current = en.Current;
                if (current.StartsAt < accumulated.EndsAt)
                {
                    var updatedAtDiff = (current.UpdatedAt - accumulated.UpdatedAt).Ticks;
                    if (updatedAtDiff == 0)
                    {
                        // Take union.
                        if (current.EndsAt > accumulated.EndsAt)
                        {
                            accumulated = accumulated with { EndsAt = current.EndsAt };
                        }
                    }
                    else if (updatedAtDiff > 0)
                    {
                        // Dispose accumulated.
                        accumulated = current;
                    }
                }
                else
                {
                    yield return accumulated;
                    accumulated = current;
                }
            }
            yield return accumulated;
            yield break;
        }

        /// <summary>
        /// 連続した区間を結合する.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<KnoqRoom> UnionContiguous(this IEnumerable<KnoqRoom> enumerable)
        {
            using var en = enumerable.GetEnumerator();
            if (!en.MoveNext())
            {
                yield break;
            }
            var accumulated = en.Current;
            while (en.MoveNext())
            {
                var current = en.Current;
                if (current.StartsAt == accumulated.EndsAt)
                {
                    accumulated = accumulated with
                    {
                        EndsAt = current.EndsAt,
                        UpdatedAt = (current.UpdatedAt > accumulated.UpdatedAt) ? current.UpdatedAt : accumulated.UpdatedAt
                    };
                }
                else
                {
                    yield return accumulated;
                    accumulated = current;
                }
            }
            yield return accumulated;
            yield break;
        }

        public static KnoqRoom KnoqResponseToRoom(this Knoq.Model.ResponseRoom room)
        {
            return new KnoqRoom(
                room.RoomId,
                room.Place,
                DateTimeOffset.Parse(room.TimeStart),
                DateTimeOffset.Parse(room.TimeEnd),
                DateTimeOffset.Parse(room.UpdatedAt)
            );
        }

        public static Room KnoqRoomToDomainRoom(this KnoqRoom room)
        {
            return new Room(
                room.Place,
                room.StartsAt,
                room.EndsAt
            );
        }
    }
}
