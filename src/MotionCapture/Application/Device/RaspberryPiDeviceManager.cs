using System.Device.Gpio;

namespace MotionCapture.Application.Device
{
    public class RaspberryPiDeviceManager : BackgroundService, IDeviceManager
    {
        private readonly ILogger<RaspberryPiDeviceManager> _logger;

        // TODO: Make these Options?
        private readonly int _redLedPin = 27;
        private readonly int _yellowLedPin = 22;
        private readonly int _greenLedPin = 4;
        private readonly int _buttonPin = 17;

        private PinValue _lastButtonPinValue = PinValue.Low;
        private bool _initializeComplete = false;

        private Lazy<GpioController> _gpioController = new Lazy<GpioController>(() => new());
        private List<Func<CancellationToken, Task>> _buttonPressCallbacks = new();

        public RaspberryPiDeviceManager(ILogger<RaspberryPiDeviceManager> logger)
        {
            _logger = logger;
        }

        public async Task WaitForInitialize()
        {
            while (!_initializeComplete) { await Task.Delay(100); }
        }

        public void DisableLed(LedColor color)
        {
            _gpioController.Value.Write(GetPinNumber(color), PinValue.Low);
        }

        public void EnableLed(LedColor color)
        {
            _gpioController.Value.Write(GetPinNumber(color), PinValue.High);
        }

        private int GetPinNumber(LedColor color)
        {
            switch (color)
            {
                case LedColor.Green:
                    return _greenLedPin;
                case LedColor.Red:
                    return _redLedPin;
                case LedColor.Yellow:
                    return _yellowLedPin;
                default:
                    throw new Exception($"Unsupported {nameof(LedColor)} - {color}");
            }
        }

        public void ToggleLed(LedColor color, bool enable)
        {
            if (enable) { EnableLed(color); }
            else { DisableLed(color); }  
        }

        public void RegisterForButtonPressCallback(Func<CancellationToken, Task> func)
        {
            _buttonPressCallbacks.Add(func);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RaspberryPiDeviceManager service is starting.");
            Initialize();

            while (!stoppingToken.IsCancellationRequested)
            {
                // *********************************************************
                // Manage button state
                var pinValue = _gpioController.Value.Read(_buttonPin);

                if (pinValue == PinValue.High && _lastButtonPinValue == PinValue.Low) // Button was pressed
                {
                    for (var i = 0; i < _buttonPressCallbacks.Count; i++) // Thread safe - other threads can add to the list, but nothing can remove from it.
                    {
                        await _buttonPressCallbacks[i](stoppingToken);
                    }

                    _logger.LogTrace($"OnButtonPress fired.");
                }

                // Because we're getting button pin value every x milliseconds, we track the last pin value so
                // we can detect when the button went from low to high and trigger our button press callbacks.
                _lastButtonPinValue = pinValue;

                await Task.Delay(50);
            }

            // Turn off hardware

            DisableLed(LedColor.Green);
            DisableLed(LedColor.Red);
            DisableLed(LedColor.Yellow);
        }

        private void Initialize()
        {
            _gpioController.Value.OpenPin(_greenLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_redLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_yellowLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_buttonPin, PinMode.InputPullDown);

            DisableLed(LedColor.Green);
            DisableLed(LedColor.Red);
            DisableLed(LedColor.Yellow);
            
            _initializeComplete = true;
        }
    }
}
