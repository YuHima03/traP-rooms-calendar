
using Microsoft.Extensions.Options;

namespace RoomsCalendar.Server.Services
{
    sealed class MemoryMonitoringService(
        ILogger<MemoryMonitoringService> logger,
        ILogger<PeriodicBackgroundService> baseLogger
        )
        : PeriodicBackgroundService(DefaultOptions, baseLogger)
    {
        static readonly IOptions<MemoryMonitoringServiceOptions> DefaultOptions = Options.Create(new MemoryMonitoringServiceOptions());

        protected override ValueTask ExecuteCoreAsync(CancellationToken ct)
        {
            var totalMemory = GC.GetTotalMemory(false);
            if (totalMemory > DefaultOptions.Value.ThresholdBytes)
            {
                logger.LogWarning("High memory usage detected: {TotalMemory} MiB.", (double)totalMemory / 1024 / 1024);
                GC.Collect();
            }
            return ValueTask.CompletedTask;
        }
    }

    sealed class MemoryMonitoringServiceOptions : IPeriodicBackgroundServiceOptions
    {
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(30);

        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(30);

        public bool RecoverOnException { get; set; } = true;

        public int ThresholdBytes { get; set; } = 50 * 1024 * 1024; // 50 MB
    }
}
