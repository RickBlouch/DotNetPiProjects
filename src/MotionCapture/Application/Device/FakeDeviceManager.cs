﻿namespace MotionCapture.Application.Device
{
    public class FakeDeviceManager : BackgroundService, IDeviceManager
    {
        private readonly ILogger<FakeDeviceManager> _logger;

        public FakeDeviceManager(ILogger<FakeDeviceManager> logger)
        {
            _logger = logger;
        }

        public void DisableLed(LedColor color)
        {
            return;
        }

        public void EnableLed(LedColor color)
        {
            return;
        }

        public void RegisterForButtonPressCallback(Func<CancellationToken, Task> func)
        {
            return;
        }

        public void ToggleLed(LedColor color, bool enable)
        {
            return;
        }

        public Task WaitForInitialize()
        {
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FakeDeviceManager service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {


                await Task.Delay(1000);
            }
        }
    }
}