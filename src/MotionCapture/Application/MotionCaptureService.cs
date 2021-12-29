using MotionCapture.Application.Device;

namespace MotionCapture.Application
{
    public class MotionCaptureService : BackgroundService
    {
        private readonly ILogger<MotionCaptureService> _logger;
        private readonly IDeviceManager _deviceManager;

        //private bool _ledBlinkEnabled = false;
        //private bool _ledOn = false;

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
            _deviceManager.RegisterForMontionSensorCallback(OnMotionStarted, OnMotionStopped);

            _deviceManager.EnableLed(LedColor.Green);
            //_deviceManager.EnableLed(LedColor.Yellow);

            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_ledBlinkEnabled)
                //{
                //    _ledOn = !_ledOn;
                //    _deviceManager.ToggleLed(LedColor.Yellow, _ledOn);
                //}
                //else if (_ledOn)
                //{
                //    _logger.LogDebug("ledBlinkEnabled = false, but the LED is still on.  Turning it LED...");
                //    _deviceManager.DisableLed(LedColor.Yellow);
                //    _ledOn = false;
                //}

                await Task.Delay(250);
            }
        }

        private Task OnButtonPress(CancellationToken token)
        {
            //_ledBlinkEnabled = !_ledBlinkEnabled;
            //_logger.LogDebug($"OnButtonPress, ledBlinkEnabled: {_ledBlinkEnabled}, ledOn: {_ledOn}");

            //Task.Run(() => );
            //Task.Run(async () =>
            //{
            //    _deviceManager.EnableLed(LedColor.Yellow);
            //    //await _deviceManager.StartVideoFpsCalc();
            //    //await _deviceManager.StartPictures();
            //    await _deviceManager.StartVideo();
            //    _deviceManager.DisableLed(LedColor.Yellow);
            //});

            return Task.CompletedTask;
        }

        private Task OnMotionStarted(CancellationToken token)
        {
            _logger.LogInformation("OnMotionStarted handler.");

            _deviceManager.EnableLed(LedColor.Red);
            _deviceManager.DisableLed(LedColor.Green);

            Task.Run(async () =>
            {
                _deviceManager.EnableLed(LedColor.Yellow);
                //await _deviceManager.StartVideoFpsCalc();
                //await _deviceManager.StartPictures();
                await _deviceManager.StartVideo();
                _deviceManager.DisableLed(LedColor.Yellow);
            });

            return Task.CompletedTask;
        }

        private Task OnMotionStopped(CancellationToken token)
        {
            _logger.LogInformation("OnMotionStopped handler.");

            _deviceManager.StopCamera();

            _deviceManager.DisableLed(LedColor.Red);
            _deviceManager.EnableLed(LedColor.Green);

            return Task.CompletedTask;
        }
    }
}
