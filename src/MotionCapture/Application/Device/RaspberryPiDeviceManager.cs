using System.Device.Gpio;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Components;
using MMALSharp.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;
using MotionCapture.Application.Device.Mmal;

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
        private readonly int _motionSensorPin = 23;
        private readonly int _motionSensorStoppedLag = 5; // Seconds

        private bool _initializeComplete = false;
        private DateTime? _motionStartedTimestampUtc = null;

        private PinValue _lastButtonPinValue = PinValue.Low;
        private PinValue _lastMotionSensorPinValue = PinValue.Low;

        private Lazy<GpioController> _gpioController = new Lazy<GpioController>(() => new());
        private List<Func<CancellationToken, Task>> _buttonPressCallbacks = new();
        private List<Func<CancellationToken, Task>> _motionStartedCallbacks = new();
        private List<Func<CancellationToken, Task>> _motionStoppedCallbacks = new();

        private MMALCamera? _camera;
        CancellationTokenSource? _cameraCaptureCts;

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

        public void RegisterForButtonPressCallback(Func<CancellationToken, Task> onButtonPress)
        {
            _buttonPressCallbacks.Add(onButtonPress);
        }

        public void RegisterForMontionSensorCallback(Func<CancellationToken, Task> onMotionStarted, Func<CancellationToken, Task> onMotionStopped)
        {
            _motionStartedCallbacks.Add(onMotionStarted);
            _motionStoppedCallbacks.Add(onMotionStopped);
        }

        public async Task StartPictures()
        {
            if (_camera == null) { _logger.LogInformation("Camera is null.");  return; }

            _logger.LogInformation("Starting picture capture");

            //using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/Pictures/MotionCapture/", "jpg"))
            //{
            //    await _camera.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);
            //}


            //////////////////////////////

            //using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/Pictures/MotionCapture/", "jpg"))
            //using (var splitter = new MMALSplitterComponent())
            //using (var imgEncoder = new MMALImageEncoder(continuousCapture: true))
            //using (var nullSink = new MMALNullSinkComponent())
            //{
            //    _camera.ConfigureCameraSettings();

            //    var portConfig = new MMALPortConfig(MMALEncoding.JPEG, MMALEncoding.I420, 90);

            //    // Create our component pipeline.         
            //    imgEncoder.ConfigureOutputPort(portConfig, imgCaptureHandler);

            //    _camera.Camera.VideoPort.ConnectTo(splitter);
            //    splitter.Outputs[0].ConnectTo(imgEncoder);
            //    _camera.Camera.PreviewPort.ConnectTo(nullSink);

            //    // Camera warm up time
            //    await Task.Delay(2000);

            //    _cameraCaptureCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            //    // Process images for x seconds.        
            //    await _camera.ProcessAsync(_camera.Camera.VideoPort, _cameraCaptureCts.Token);

            //}

            ////////////////

            var folder = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");

            using (var imgCaptureHandler = new CustomImageStreamCaptureHandler($"/home/pi/Pictures/MotionCapture/{folder}/Capture.jpg"))
            {

                // hack set _increment here to 100?  all images will start with 100 then and increment up.
                // OR Override FileStreamCaptureHandler and implement NewFile method.

                _cameraCaptureCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                await _camera.TakePictureTimeout(
                    imgCaptureHandler,
                    MMALEncoding.JPEG,
                    MMALEncoding.I420,
                    _cameraCaptureCts.Token,
                    true);
            }


            // Camera TODOs
            //  - Figure out how to capture 10 frames per second with highest quality possible
            //  - Figure out what camera 'warmup' time is and if the camera can be persistently warmed up instead of warming up when it's time to capture
            //  - Figure out if it makes sense to use the camera to detect motion instead of using the PIR sensor (can we measure power usage of both?)

        }

        public void StopPictures()
        {
            _logger.LogInformation("Cancelling picture capture");
            _cameraCaptureCts?.Cancel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RaspberryPiDeviceManager service is starting.");
            Initialize();

            while (!stoppingToken.IsCancellationRequested)
            {
                // *********************************************************
                // Manage PIR motion sensor state

                var motionSensorPinValue = _gpioController.Value.Read(_motionSensorPin);

                if (motionSensorPinValue == PinValue.High)
                {
                    if (!_motionStartedTimestampUtc.HasValue)
                    {
                        for (var i = 0; i < _motionStartedCallbacks.Count; i++)
                        {
                            await _motionStartedCallbacks[i](stoppingToken);
                        }

                        _logger.LogTrace($"OnMotionStarted fired.");
                    }

                    _motionStartedTimestampUtc = DateTime.UtcNow;
                }
                else if (_motionStartedTimestampUtc.HasValue) //PinValue.Low
                {
                    var diff = DateTime.UtcNow - _motionStartedTimestampUtc.Value;

                    if (diff.TotalSeconds > _motionSensorStoppedLag)
                    {
                        for (var i = 0; i < _motionStoppedCallbacks.Count; i++)
                        {
                            await _motionStoppedCallbacks[i](stoppingToken);
                        }

                        _logger.LogTrace("OnMotionStopped fired.");

                        _motionStartedTimestampUtc = null;
                    }
                }

                // *********************************************************
                // Manage button state

                var buttonPinValue = _gpioController.Value.Read(_buttonPin);

                if (buttonPinValue == PinValue.High && _lastButtonPinValue == PinValue.Low) // Button was pressed
                {
                    for (var i = 0; i < _buttonPressCallbacks.Count; i++) // Thread safe - other threads can add to the list, but nothing can remove from it.
                    {
                        await _buttonPressCallbacks[i](stoppingToken);
                    }

                    _logger.LogTrace($"OnButtonPress fired.");
                }

                // Because we're getting button pin value every x milliseconds, we track the last pin value so
                // we can detect when the button went from low to high and trigger our button press callbacks.
                _lastButtonPinValue = buttonPinValue;

                await Task.Delay(50);
            }

            Reset();

            _camera?.Cleanup();
        }

        private void Initialize()
        {
            _gpioController.Value.OpenPin(_greenLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_redLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_yellowLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_buttonPin, PinMode.InputPullDown);
            _gpioController.Value.OpenPin(_motionSensorPin, PinMode.Input);

            _camera = MMALCamera.Instance;

            MMALCameraConfig.ExposureCompensation = (int)MMAL_PARAM_EXPOSUREMODE_T.MMAL_PARAM_EXPOSUREMODE_SPORTS;
            //MMALCameraConfig.StillBurstMode = true;
            MMALCameraConfig.StillResolution = new Resolution(640, 480); // Set to 640 x 480. Default is 1280 x 720.
            //MMALCameraConfig.StillResolution = new Resolution(800, 600); // Set to 640 x 480. Default is 1280 x 720.
            MMALCameraConfig.StillFramerate = new MMAL_RATIONAL_T(30, 1); // Set to 20fps. Default is 30fps.
            //MMALCameraConfig.ShutterSpeed = 2000000; // Set to 2s exposure time. Default is 0 (auto).
            //MMALCameraConfig.ISO = 400; // Set ISO to 400. Default is 0 (auto).

            Reset();

            _initializeComplete = true;
        }

        public void Reset()
        {
            DisableLed(LedColor.Green);
            DisableLed(LedColor.Red);
            DisableLed(LedColor.Yellow);
        }
    }
}
