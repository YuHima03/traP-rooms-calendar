namespace RoomsCalendar.Server.Configurations
{
    sealed class KnoqClientOptions
    {
        [ConfigurationKeyName("KNOQ_API_BASE_ADDRESS")]
        public string? KnoqApiBaseAddress { get; set; }
    }
}
