using Microsoft.Extensions.Options;
using RoomsCalendar.Share;
using RoomsCalendar.Share.Domain;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    sealed class TitechRoomsCollector(
        [FromKeyedServices(RoomProviderNames.TitechReserved)] IRoomsProvider reservedRoomsProvider,
        [FromKeyedServices(RoomProviderNames.TitechVacant)] IRoomsProvider vacantRoomsProvider,
        IHttpClientFactory httpClientFactory,
        IOptions<TitechRoomsCollectorOptions> options,
        ILogger<TitechRoomsCollector> logger,
        ILogger<PeriodicBackgroundService> baseLogger
        )
        : PeriodicBackgroundService(options, baseLogger)
    {
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        const string DateTimeParseFormat = "yyyyMMddHHmm";

        const string MatchEventName = "サークル活動：デジタル創作同好会traP";

        readonly AngleSharp.Html.Parser.HtmlParserOptions HtmlParserOptions = new()
        {
            IsScripting = false,
            IsStrictMode = false,
        };

        Uri? SourceUri { get; set; }

        Uri? RequestUri
        {
            get
            {
                if (SourceUri is null)
                {
                    return null;
                }
                var timeZoneToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, options.Value.TimeZoneInfo).Date;
                UriBuilder ub = new(SourceUri)
                {
                    Query = $"date={timeZoneToday:yyyyMMdd}&dateto={timeZoneToday.AddDays(6):yyyyMMdd}&b=3&nofilter=1&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
                };
                return ub.Uri;
            }
        }

        protected override async ValueTask ExecuteCoreAsync(CancellationToken ct)
        {
            var reqUri = RequestUri;
            if (reqUri is null)
            {
                return;
            }
            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(reqUri, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("HTTP request to {SourceUri} failed with status code {StatusCode}.", SourceUri, response.StatusCode);
                }
                using var rooms = await ParseFetchResultAsync(await response.Content.ReadAsStreamAsync(ct), ct);
                if (rooms.Size != 0)
                {
                    var buffer = ArrayPool<Room>.Shared.Rent(rooms.Size);
                    try
                    {
                        var timeZoneToday = TimeZoneInfo.ConvertTimeToUtc(
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, options.Value.TimeZoneInfo).Date,
                            options.Value.TimeZoneInfo
                        );

                        var reservedCnt = rooms
                            .AsValueEnumerable()
                            .Where(r => MatchEventName.Equals(r.EventName, StringComparison.InvariantCultureIgnoreCase))
                            .Select(r => r.ToDomainRoom())
                            .CopyTo(buffer.AsSpan());
                        await reservedRoomsProvider.UpdateRoomsAsync((ReadOnlySpan<Room>)buffer.AsSpan(0, reservedCnt), timeZoneToday, ct);

                        var vacantCnt = rooms
                            .AsValueEnumerable()
                            .Where(r => string.IsNullOrWhiteSpace(r.EventName))
                            .Select(r => r.ToDomainRoom())
                            .CopyTo(buffer.AsSpan());
                        await vacantRoomsProvider.UpdateRoomsAsync((ReadOnlySpan<Room>)buffer.AsSpan(0, vacantCnt), timeZoneToday, ct);
                    }
                    finally
                    {
                        ArrayPool<Room>.Shared.Return(buffer, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching rooms data.");
            }
        }

        protected override async ValueTask InitializeCoreAsync(CancellationToken ct)
        {
            if (Uri.TryCreate(options.Value.SourceUrl, UriKind.Absolute, out var uri))
            {
                SourceUri = uri;
            }
            else
            {
                logger.LogWarning("Source URL is not set or invalid: {SourceUrl}", options.Value.SourceUrl);
            }

            if (options.Value.Delay > TimeSpan.FromMinutes(10))
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), ct);
                    await ExecuteCoreAsync(ct);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during the initial fetch of rooms data.");
                }
            }
        }

        async ValueTask<PooledArray<TitechRoom>> ParseFetchResultAsync(Stream content, CancellationToken ct = default)
        {
            using var doc = await new AngleSharp.Html.Parser.HtmlParser(HtmlParserOptions).ParseDocumentAsync(content, ct);
            var mainContainer = doc.GetElementById("div");
            if (mainContainer is null)
            {
                return new();
            }
            var timeZoneInfo = options.Value.TimeZoneInfo;
            var timeZoneOffset = timeZoneInfo.BaseUtcOffset;
            return mainContainer.GetElementsByClassName("trRoom")
                .AsValueEnumerable()
                .SelectMany(e =>
                {
                    var placeName = e.GetElementsByClassName("spshow")
                        .FirstOrDefault(e => e.TagName.Equals(AngleSharp.Dom.TagNames.Th, StringComparison.OrdinalIgnoreCase))?
                        .GetElementsByTagName(AngleSharp.Dom.TagNames.Br)
                        .FirstOrDefault()?
                        .NextSibling?
                        .TextContent
                        .Trim();
                    return e.GetElementsByTagName(AngleSharp.Dom.TagNames.Td)
                        .AsValueEnumerable()
                        .Where(e => string.IsNullOrWhiteSpace(e.TextContent) || e.TextContent.AsSpan().Trim().Equals(MatchEventName, StringComparison.OrdinalIgnoreCase))
                        .Select(e =>
                        {
                            var mergeKeyString = e.GetAttribute("data-merge");
                            var dateString = e.GetAttribute("data-date");
                            Span<char> strBuffer = stackalloc char[DateTimeParseFormat.Length];
                            DateTimeOffset timeStart = TimeZoneInfo.ConvertTimeToUtc(
                                DateTime.ParseExact(GetDateTimeString(strBuffer, dateString, e.GetAttribute("data-timefrom")), DateTimeParseFormat, null),
                                timeZoneInfo
                            );
                            DateTimeOffset timeEnd = TimeZoneInfo.ConvertTimeToUtc(
                                DateTime.ParseExact(GetDateTimeString(strBuffer, dateString, e.GetAttribute("data-timeto")), DateTimeParseFormat, null),
                                timeZoneInfo
                            );
                            return new TitechRoom
                            {
                                EventName = e.TextContent.Trim(),
                                PlaceName = placeName!,
                                MergeKey = string.IsNullOrWhiteSpace(mergeKeyString) ? 0L : long.Parse(mergeKeyString),
                                TimeFrom = timeStart.ToOffset(timeZoneOffset),
                                TimeTo = timeEnd.ToOffset(timeZoneOffset)
                            };
                        })
                        .Where(r => !string.IsNullOrWhiteSpace(r.PlaceName))
                        .SelectMerged(
                            determiner: (x, y) => x.EventName == y.EventName,
                            aggregator: (x, y) => new TitechRoom
                            {
                                EventName = x.EventName,
                                PlaceName = x.PlaceName,
                                MergeKey = x.MergeKey,
                                TimeFrom = x.TimeFrom < y.TimeFrom ? x.TimeFrom : y.TimeFrom,
                                TimeTo = x.TimeTo > y.TimeTo ? x.TimeTo : y.TimeTo
                            }
                        );
                })
                .ToArrayPool();

            static Span<char> GetDateTimeString(Span<char> buffer, string? date, string? time)
            {
                buffer.Fill('0');
                if (!string.IsNullOrEmpty(date))
                {
                    date.CopyTo(buffer);
                }
                if (!string.IsNullOrEmpty(time))
                {
                    if (time.Length < 4)
                    {
                        time.CopyTo(buffer[^time.Length..]);
                    }
                    else
                    {
                        time.TryCopyTo(buffer[^4..]);
                    }
                }
                return buffer;
            }
        }

        readonly struct TitechRoom
        {
            public string? EventName { get; init; }

            public long MergeKey { get; init; }

            public required string PlaceName { get; init; }

            public DateTimeOffset TimeFrom { get; init; }

            public DateTimeOffset TimeTo { get; init; }

            public Room ToDomainRoom()
            {
                return new Room(PlaceName, TimeFrom, TimeTo);
            }
        }
    }

    sealed class TitechRoomsCollectorOptions : IPeriodicBackgroundServiceOptions
    {
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;

        public TimeSpan FetchInterval { get; set; } = TimeSpan.FromHours(6);

        public string? SourceUrl { get; set; }

        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Utc;

        TimeSpan IPeriodicBackgroundServiceOptions.Period => FetchInterval;

        bool IPeriodicBackgroundServiceOptions.RecoverOnException => false;
    }

    file static class ValueEnumerableExtensions
    {
        public static ValueEnumerable<SelectMergedItr<TEnumerator, T>, T> SelectMerged<TEnumerator, T>(this ValueEnumerable<TEnumerator, T> source, Func<T, T, bool> determiner, Func<T, T, T> aggregator)
            where TEnumerator : struct, IValueEnumerator<T>, allows ref struct
        {
            return new ValueEnumerable<SelectMergedItr<TEnumerator, T>, T>(
                new SelectMergedItr<TEnumerator, T>(source.Enumerator, determiner, aggregator)
            );
        }

        public ref struct SelectMergedItr<TEnumerator, TSource> : IValueEnumerator<TSource>
            where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
        {
            TEnumerator source;
            readonly Func<TSource, TSource, bool> determiner;
            readonly Func<TSource, TSource, TSource> aggregator;

            byte status = 0; // 0: not initialized, 1: initialized, 2: completed
            TSource accumulate;

            public SelectMergedItr(TEnumerator source, Func<TSource, TSource, bool> determiner, Func<TSource, TSource, TSource> aggregator)
            {
                this.source = source;
                this.determiner = determiner;
                this.aggregator = aggregator;
                Unsafe.SkipInit(out accumulate);
            }

            public readonly void Dispose()
            {
                source.Dispose();
            }

            public readonly bool TryCopyTo(scoped Span<TSource> destination, Index offset)
            {
                return false;
            }

            public bool TryGetNext(out TSource current)
            {
                switch (status)
                {
                    case 0:
                        if (!source.TryGetNext(out accumulate))
                        {
                            Unsafe.SkipInit(out current);
                            return false;
                        }
                        status = 1;
                        goto case 1;

                    case 1:
                        while (source.TryGetNext(out var next))
                        {
                            if (!determiner(accumulate, next))
                            {
                                current = accumulate;
                                accumulate = next;
                                return true;
                            }
                            else
                            {
                                accumulate = aggregator(accumulate, next);
                            }
                        }
                        status = 2;
                        current = accumulate;
                        return true;

                    default:
                        Unsafe.SkipInit(out current);
                        return false;
                }
            }

            public readonly bool TryGetNonEnumeratedCount(out int count)
            {
                count = default;
                return false;
            }

            public readonly bool TryGetSpan(out ReadOnlySpan<TSource> span)
            {
                span = default;
                return false;
            }
        }
    }
}
