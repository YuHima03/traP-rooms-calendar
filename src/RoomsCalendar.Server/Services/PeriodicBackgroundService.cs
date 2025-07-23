using Microsoft.Extensions.Options;

namespace RoomsCalendar.Server.Services
{
    abstract class PeriodicBackgroundService(
        IOptions<IPeriodicBackgroundServiceOptions> options,
        ILogger<PeriodicBackgroundService> logger
        )
        : BackgroundService
    {
        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var optn = options.Value;
                await Task.WhenAll(
                    Task.Delay(optn.Delay, stoppingToken),
                    InitializeCoreAsync(stoppingToken).AsTask()
                );
                using PeriodicTimer? timer = new(optn.Period);
                do
                {
                    try
                    {
                        await ExecuteCoreAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        if (!optn.RecoverOnException)
                        {
                            throw;
                        }
                        logger.LogError(ex, "An error occurred while executing periodic background service.");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        protected abstract ValueTask ExecuteCoreAsync(CancellationToken ct);

        protected virtual ValueTask InitializeCoreAsync(CancellationToken ct) => ValueTask.CompletedTask;
    }

    interface IPeriodicBackgroundServiceOptions
    {
        TimeSpan Delay { get; }

        TimeSpan Period { get; }

        bool RecoverOnException { get; }
    }
}
