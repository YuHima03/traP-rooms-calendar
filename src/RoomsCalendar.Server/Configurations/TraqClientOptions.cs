namespace RoomsCalendar.Server.Configurations
{
    public class TraqClientOptions
    {
        [ConfigurationKeyName("TRAQ_API_BASE_ADDRESS")]
        public string? TraqApiBaseAddress { get; set; }

        [ConfigurationKeyName("TRAQ_COOKIE_TOKEN")]
        public string? TraqCookieAuthenticationToken { get; set; }
    }
}
