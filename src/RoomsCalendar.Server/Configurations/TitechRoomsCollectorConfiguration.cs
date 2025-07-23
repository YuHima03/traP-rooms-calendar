using RoomsCalendar.Server.Services;

namespace RoomsCalendar.Server.Configurations
{
    sealed class TitechRoomsCollectorConfiguration
    {
        [ConfigurationKeyName("TITECH_ROOMS_SOURCE_ADDRESS")]
        public string? SourceUrl { get; set; }

        public void ConfigureTitechRoomsCollectorOptions(TitechRoomsCollectorOptions destination, TimeZoneInfo? timeZoneInfo = null)
        {
            timeZoneInfo ??= TimeZoneInfo.Utc;
            var nowTimeOfDay = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo).TimeOfDay;
#if DEBUG
            var delay = TimeSpan.FromSeconds(20);
#else
            var delay = TimeSpan.FromHours(6 - (nowTimeOfDay.Hours % 6)) - nowTimeOfDay.Subtract(TimeSpan.FromHours(nowTimeOfDay.Hours));
#endif

            destination.Delay = delay;
            destination.FetchInterval = TimeSpan.FromHours(6);
            destination.SourceUrl = SourceUrl;
            if (TimeZoneInfo.TryFindSystemTimeZoneById("Tokyo Standard Time", out var jst))
            {
                destination.TimeZoneInfo = jst;
            }
        }
    }
}
