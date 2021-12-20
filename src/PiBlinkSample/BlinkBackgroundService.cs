using System.Device.Gpio;

namespace PiBlinkSample
{
    public class BlinkBackgroundService : BackgroundService
    {
        private readonly ILogger<BlinkBackgroundService> _logger;

        private int _ledPin = 4;
        private int _buttonPin = 17;

        private bool _ledBlinkEnabled = false;
        private bool _ledOn = false;
        private PinValue _lastButtonPinValue = PinValue.Low;
        
        public BlinkBackgroundService(ILogger<BlinkBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BlinkBackgroundService service is starting.");
            stoppingToken.Register(() => _logger.LogInformation("BlinkBackgroundService stoppingToken was cancelled"));

           
            using var pinController = new GpioController();

            pinController.OpenPin(_ledPin, PinMode.Output);
            pinController.OpenPin(_buttonPin, PinMode.InputPullDown);

            // Set led off to match initial state.
            pinController.Write(_ledPin, PinValue.Low);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_ledBlinkEnabled)
                {
                    _ledOn = !_ledOn;
                    pinController.Write(_ledPin, _ledOn ? PinValue.High : PinValue.Low);
                }
                else if (_ledOn)
                {
                    _logger.LogInformation($"ledBlinkEnabled = false, but the LED is still on.  Turning it LED...");
                    pinController.Write(_ledPin, PinValue.Low);
                    _ledOn = false;
                }

                var pinValue = pinController.Read(_buttonPin);

                if (pinValue == PinValue.High && _lastButtonPinValue == PinValue.Low) // Button was pressed
                {
                    _ledBlinkEnabled = !_ledBlinkEnabled;
                    _logger.LogInformation($"OnButtonDown, ledBlinkEnabled: {_ledBlinkEnabled}, ledOn: {_ledOn}");
                }

                _lastButtonPinValue = pinValue;


                await Task.Delay(50);
            }

            _logger.LogInformation("BlinkBackgroundService service is stopping.");
        }
    }
}
