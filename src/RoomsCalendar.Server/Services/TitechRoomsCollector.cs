using Microsoft.Extensions.Options;
using RoomsCalendar.Share.Domain;
using System.Diagnostics.CodeAnalysis;
using ZLinq;

namespace RoomsCalendar.Server.Services
{
    class TitechRoomsCollector(
        TitechRoomsProvider dataProvider,
        IHttpClientFactory httpClientFactory,
        IOptions<TitechRoomsCollectorOptions> options,
        ILogger<TitechRoomsCollector> logger,
        ILogger<PeriodicBackgroundService> baseLogger
        )
        : PeriodicBackgroundService(options, baseLogger)
    {
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        const string DateTimeParseFormat = "yyyyMMddHHmm";

        const string MatchTdContent = "サークル活動：デジタル創作同好会traP";

        readonly AngleSharp.Html.Parser.HtmlParser HtmlParser = new(new AngleSharp.Html.Parser.HtmlParserOptions
        {
            IsScripting = false
        });

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
                var response = await httpClient.GetAsync(reqUri, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("HTTP request to {SourceUri} failed with status code {StatusCode}.", SourceUri, response.StatusCode);
                }
                var rooms = await ParseFetchResultAsync(await response.Content.ReadAsStreamAsync(ct), ct);
                if (rooms.Length != 0)
                {
                    var timeZoneToday = TimeZoneInfo.ConvertTimeToUtc(
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, options.Value.TimeZoneInfo).Date,
                        options.Value.TimeZoneInfo
                    );
                    await dataProvider.UpdateRoomsAsync(rooms, timeZoneToday, ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching rooms data.");
            }
        }

        protected override ValueTask InitializeCoreAsync(CancellationToken ct)
        {
            if (Uri.TryCreate(options.Value.SourceUrl, UriKind.Absolute, out var uri))
            {
                SourceUri = uri;
            }
            else
            {
                logger.LogWarning("Source URL is not set or invalid: {SourceUrl}", options.Value.SourceUrl);
            }
            return ValueTask.CompletedTask;
        }

        async ValueTask<Room[]> ParseFetchResultAsync(Stream content, CancellationToken ct = default)
        {
            var doc = await HtmlParser.ParseDocumentAsync(content, ct);
            var mainContainer = doc.GetElementById("div");
            if (mainContainer is null)
            {
                return [];
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
                        .Where(e => e.TextContent == MatchTdContent)
                        .GroupBy(e => e.GetAttribute("data-merge"))
                        .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Any())
                        .Select(g =>
                        {
                            var (first, last) = (g.First(), g.Last());
                            DateTimeOffset timeStart = TimeZoneInfo.ConvertTimeToUtc(
                                DateTime.ParseExact(first.GetAttribute("data-date") + first.GetAttribute("data-timefrom"), DateTimeParseFormat, null),
                                timeZoneInfo
                            );
                            DateTimeOffset timeEnd = TimeZoneInfo.ConvertTimeToUtc(
                                DateTime.ParseExact(last.GetAttribute("data-date") + last.GetAttribute("data-timeto"), DateTimeParseFormat, null),
                                timeZoneInfo
                            );
                            return new Room(placeName!, timeStart.ToOffset(timeZoneOffset), timeEnd.ToOffset(timeZoneOffset));
                        })
                        .Where(r => !string.IsNullOrWhiteSpace(r.PlaceName));
                })
                .ToArray();
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
}
