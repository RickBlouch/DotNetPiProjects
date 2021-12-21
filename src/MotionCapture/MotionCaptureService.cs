using MotionCapture.Application.Device;

namespace MotionCapture
{
    public class MotionCaptureService : BackgroundService
    {
        private readonly ILogger<MotionCaptureService> _logger;
        private readonly IDeviceManager _deviceManager;

        private bool _ledBlinkEnabled = false;
        private bool _ledOn = false;
        
        public MotionCaptureService(ILogger<MotionCaptureService> logger, IDeviceManager deviceManager)
        {
            _logger = logger;
            _deviceManager = deviceManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MotionCapture service is starting.");
            //stoppingToken.Register(() => _logger.LogInformation("BlinkBackgroundService stoppingToken was cancelled"));

            await _deviceManager.WaitForInitialize();

            _deviceManager.RegisterForButtonPressCallback(OnButtonPress);
            _deviceManager.DisableLed(LedColor.Green);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_ledBlinkEnabled)
                {
                    _ledOn = !_ledOn;
                    _deviceManager.ToggleLed(LedColor.Green, _ledOn);
                }
                else if (_ledOn)
                {
                    _logger.LogDebug("ledBlinkEnabled = false, but the LED is still on.  Turning it LED...");
                    _deviceManager.DisableLed(LedColor.Green);
                    _ledOn = false;
                }

                await Task.Delay(250);
            }
        }

        private Task OnButtonPress(CancellationToken token)
        {
            _ledBlinkEnabled = !_ledBlinkEnabled;
            _logger.LogInformation($"OnButtonPress, ledBlinkEnabled: {_ledBlinkEnabled}, ledOn: {_ledOn}");

            return Task.CompletedTask;
        }
    }
}
