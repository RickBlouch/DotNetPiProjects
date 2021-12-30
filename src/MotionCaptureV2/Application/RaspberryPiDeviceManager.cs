using System.Device.Gpio;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Components;
using MMALSharp.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;
using MMALSharp.Ports.Outputs;
using MMALSharp.Processors.Motion;

namespace MotionCaptureV2.Application
{
    public class RaspberryPiDeviceManager : BackgroundService
    {
        private readonly ILogger<RaspberryPiDeviceManager> _logger;
        private readonly ILoggerFactory _loggerFactory;

        // TODO: Make these Options?
        private readonly int _redLedPin = 27;
        private readonly int _yellowLedPin = 22;
        private readonly int _greenLedPin = 4;
        //private readonly int _buttonPin = 17;
        //private readonly int _motionSensorPin = 23;
        //private readonly int _motionSensorStoppedLag = 7; // Seconds

        //private DateTime? _motionStartedTimestampUtc = null;

        //private PinValue _lastButtonPinValue = PinValue.Low;
        //private PinValue _lastMotionSensorPinValue = PinValue.Low;

        private Lazy<GpioController> _gpioController = new Lazy<GpioController>(() => new());
        //private List<Func<CancellationToken, Task>> _buttonPressCallbacks = new();
        //private List<Func<CancellationToken, Task>> _motionStartedCallbacks = new();
        //private List<Func<CancellationToken, Task>> _motionStoppedCallbacks = new();

        //private MMALCamera? _camera;
        //CancellationTokenSource? _cameraCaptureCts;

        public RaspberryPiDeviceManager(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RaspberryPiDeviceManager>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RaspberryPiDeviceManager service is starting.");
            Initialize();

            EnableLed(LedColor.Green);

            await RecordMotionWithSnapshot(600, 15, stoppingToken);

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("In loop");

            //    // *********************************************************
            //    // Manage PIR motion sensor state

            //    //var motionSensorPinValue = _gpioController.Value.Read(_motionSensorPin);

            //    //if (motionSensorPinValue == PinValue.High)
            //    //{
            //    //    if (!_motionStartedTimestampUtc.HasValue)
            //    //    {
            //    //        for (var i = 0; i < _motionStartedCallbacks.Count; i++)
            //    //        {
            //    //            await _motionStartedCallbacks[i](stoppingToken);
            //    //        }

            //    //        _logger.LogTrace($"OnMotionStarted fired.");
            //    //    }

            //    //    _motionStartedTimestampUtc = DateTime.UtcNow;
            //    //}
            //    //else if (_motionStartedTimestampUtc.HasValue) //PinValue.Low
            //    //{
            //    //    var diff = DateTime.UtcNow - _motionStartedTimestampUtc.Value;

            //    //    if (diff.TotalSeconds > _motionSensorStoppedLag)
            //    //    {
            //    //        for (var i = 0; i < _motionStoppedCallbacks.Count; i++)
            //    //        {
            //    //            await _motionStoppedCallbacks[i](stoppingToken);
            //    //        }

            //    //        _logger.LogTrace("OnMotionStopped fired.");

            //    //        _motionStartedTimestampUtc = null;
            //    //    }
            //    //}

            //    // *********************************************************
            //    // Manage button state

            //    //var buttonPinValue = _gpioController.Value.Read(_buttonPin);

            //    //if (buttonPinValue == PinValue.High && _lastButtonPinValue == PinValue.Low) // Button was pressed
            //    //{
            //    //    for (var i = 0; i < _buttonPressCallbacks.Count; i++) // Thread safe - other threads can add to the list, but nothing can remove from it.
            //    //    {
            //    //        await _buttonPressCallbacks[i](stoppingToken);
            //    //    }

            //    //    _logger.LogTrace($"OnButtonPress fired.");
            //    //}

            //    //// Because we're getting button pin value every x milliseconds, we track the last pin value so
            //    //// we can detect when the button went from low to high and trigger our button press callbacks.
            //    //_lastButtonPinValue = buttonPinValue;

            //    await Task.Delay(1000);
            //}

            Reset();

            //_camera?.Cleanup();
        }

        private void Initialize()
        {
            _gpioController.Value.OpenPin(_greenLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_redLedPin, PinMode.Output);
            _gpioController.Value.OpenPin(_yellowLedPin, PinMode.Output);
            //_gpioController.Value.OpenPin(_buttonPin, PinMode.InputPullDown);
            //_gpioController.Value.OpenPin(_motionSensorPin, PinMode.Input);

            //_camera = MMALCamera.Instance;
            //MMALLog.LoggerFactory = _loggerFactory;

            //MMALCameraConfig.ExposureCompensation = (int)MMAL_PARAM_EXPOSUREMODE_T.MMAL_PARAM_EXPOSUREMODE_SPORTS;
            //MMALCameraConfig.StillBurstMode = true;
            //MMALCameraConfig.StillResolution = new Resolution(640, 480); // Set to 640 x 480. Default is 1280 x 720.
            //MMALCameraConfig.StillResolution = new Resolution(800, 600); // Set to 640 x 480. Default is 1280 x 720.
            //MMALCameraConfig.StillFramerate = new MMAL_RATIONAL_T(30, 1); // Set to 20fps. Default is 30fps.
            //MMALCameraConfig.ShutterSpeed = 2000000; // Set to 2s exposure time. Default is 0 (auto).
            //MMALCameraConfig.ISO = 400; // Set ISO to 400. Default is 0 (auto).

            //MMALLog.Logger.LogInformation("test logger");

            Reset();
        }

        public void Reset()
        {
            DisableLed(LedColor.Green);
            DisableLed(LedColor.Red);
            DisableLed(LedColor.Yellow);

            //StopCamera();
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

        //public void RegisterForButtonPressCallback(Func<CancellationToken, Task> onButtonPress)
        //{
        //    _buttonPressCallbacks.Add(onButtonPress);
        //}

        //public void RegisterForMontionSensorCallback(Func<CancellationToken, Task> onMotionStarted, Func<CancellationToken, Task> onMotionStopped)
        //{
        //    _motionStartedCallbacks.Add(onMotionStarted);
        //    _motionStoppedCallbacks.Add(onMotionStopped);
        //}

        public async Task RecordMotionWithSnapshot(int totalSeconds, int recordSeconds, CancellationToken applicationToken)
        {
            // Assumes the camera has been configured.
            var cam = MMALCamera.Instance;

            // h.264 requires key frames for the circular buffer capture handler.
            MMALCameraConfig.InlineHeaders = true;
            MMALCameraConfig.Resolution = new Resolution(640, 480);

            _logger.LogInformation("Starting motion stuff");

            using (var snapshotCaptureHandler = new FrameBufferCaptureHandler("/home/pi/Pictures/MotionCapture2", "jpg"))
            using (var videoCaptureHandler = new CircularBufferCaptureHandler(4000000, "/home/pi/Pictures/MotionCapture2", "h264"))
            using (var motionCaptureHandler = new FrameBufferCaptureHandler())
            using (var resizer = new MMALIspComponent())
            using (var splitter = new MMALSplitterComponent())
            using (var videoEncoder = new MMALVideoEncoder())
            using (var imageEncoder = new MMALImageEncoder(continuousCapture: true))  // Setting continuousCapture to true feeds every frame to the snapshotCaptureHandler
            {

                _logger.LogInformation("1");
                splitter.ConfigureInputPort(new MMALPortConfig(MMALEncoding.OPAQUE, MMALEncoding.I420), cam.Camera.VideoPort, null);
                videoEncoder.ConfigureOutputPort(new MMALPortConfig(MMALEncoding.H264, MMALEncoding.I420, 0, MMALVideoEncoder.MaxBitrateLevel4, null), videoCaptureHandler);
                imageEncoder.ConfigureOutputPort(new MMALPortConfig(MMALEncoding.JPEG, MMALEncoding.I420, quality: 90), snapshotCaptureHandler);

                // Once again, the resizer sends 640 x 480 raw frames to the motion detection handler.
                resizer.ConfigureOutputPort<VideoPort>(0, new MMALPortConfig(MMALEncoding.RGB24, MMALEncoding.RGB24, width: 640, height: 480), motionCaptureHandler);
                _logger.LogInformation("2");
                cam.Camera.VideoPort.ConnectTo(splitter);
                splitter.Outputs[0].ConnectTo(resizer);
                splitter.Outputs[1].ConnectTo(videoEncoder);
                splitter.Outputs[2].ConnectTo(imageEncoder);

                _logger.LogInformation("3");
                // Camera warm-up.
                await Task.Delay(2000);

                // We'll use the default settings for this example.
                var motionConfig = new MotionConfig(algorithm: new MotionAlgorithmRGBDiff());

                _logger.LogInformation("4");

                // Duration of the motion-detection operation.
                var motionDetectionSeconds = new CancellationTokenSource(TimeSpan.FromSeconds(totalSeconds)).Token;
                var stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(applicationToken, motionDetectionSeconds);

                snapshotCaptureHandler.WriteFrame();

                _logger.LogInformation($"Detecting motion for {totalSeconds} seconds.");

                await cam.WithMotionDetection(
                    motionCaptureHandler,
                    motionConfig,
                    // This callback will be invoked when motion has been detected.
                    async () =>
                    {
                        _logger.LogInformation("motion detected 1");

                        // When motion is detected, temporarily disable notifications
                        motionCaptureHandler.DisableMotionDetection();
                        _logger.LogInformation($"\n     {DateTime.Now:hh\\:mm\\:ss} Motion detected, recording for {recordSeconds} seconds.");

                        // Save a snapshot as soon as motion is detected
                        snapshotCaptureHandler.WriteFrame();

                        // When the recording period expires, stop recording and re-enable capture
                        var stopRecording = new CancellationTokenSource();
                        stopRecording.Token.Register(() =>
                        {
                            _logger.LogInformation($"     {DateTime.Now:hh\\:mm\\:ss} ...recording stopped.");
                            motionCaptureHandler.EnableMotionDetection();

                            // Calling split will close the h.264 file stream and open another file to
                            // store new circular buffer data while we wait for another motion event.
                            videoCaptureHandler.StopRecording();
                            videoCaptureHandler.Split();
                        });

                        // Save additional snapshots 1- and 2-seconds after motion was detected
                        var snapshotOneSecond = new CancellationTokenSource();
                        var snapshotTwoSeconds = new CancellationTokenSource();
                        snapshotOneSecond.Token.Register(snapshotCaptureHandler.WriteFrame);
                        snapshotTwoSeconds.Token.Register(snapshotCaptureHandler.WriteFrame);

                        // Start the countdowns
                        stopRecording.CancelAfter(recordSeconds * 1000);
                        snapshotOneSecond.CancelAfter(1000);
                        snapshotTwoSeconds.CancelAfter(2000);

                        // Record until the duration passes or the overall motion detection token expires
                        await Task.WhenAny(
                                    videoCaptureHandler.StartRecording(videoEncoder.RequestIFrame, stopRecording.Token),
                                    stoppingToken.Token.AsTask()
                                );

                        // Ensure all tokens are cancelled if the overall timeout expired.
                        if (!stopRecording.IsCancellationRequested)
                        {
                            stopRecording.Cancel();
                            snapshotOneSecond.Cancel();
                            snapshotTwoSeconds.Cancel();
                        }
                    })
                    .ProcessAsync(cam.Camera.VideoPort, stoppingToken.Token);
            }
            cam.Cleanup();


            _logger.LogInformation("Motion detection finished");

        }
    }
}
