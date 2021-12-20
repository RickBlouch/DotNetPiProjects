namespace PiBlinkSample
{
    public class BlinkBackgroundService : BackgroundService
    {
        private readonly ILogger<BlinkBackgroundService> _logger;

        public static int Counter = 0;

        public BlinkBackgroundService(ILogger<BlinkBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BlinkBackgroundService service is starting.");

            stoppingToken.Register(() => _logger.LogInformation("BlinkBackgroundService stoppingToken was cancelled"));

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation($"BlinkBackgroundService polled {Counter} times.");

                Counter++;

                await Task.Delay(1000);
            }

            _logger.LogInformation("BlinkBackgroundService service is stopping.");
        }

        //public Task StartAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("BlinkBackgroundService StartAsync called.");

        //    throw new NotImplementedException();
        //}

        //public Task StopAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("BlinkBackgroundService StartAsync called.");

        //    throw new NotImplementedException();
        //}
    }
}
