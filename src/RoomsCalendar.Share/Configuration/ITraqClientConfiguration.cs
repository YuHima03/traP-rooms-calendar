namespace RoomsCalendar.Share.Configuration
{
    public interface ITraqClientConfiguration
    {
        public string? TraqApiBaseAddress { get; set; }

        public string? TraqCookieAuthenticationToken { get; set; }
    }
}
