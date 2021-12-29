using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Handlers;
using MMALSharp.Processors;

namespace MotionCapture.Application.Device.Mmal
{
    /// <summary>
    /// A capture handler which stores its data to memory.
    /// </summary>
    public class FpsCaptureHandler : OutputCaptureHandler
    {
        private static DateTime _start;
        private static int _fps;
        private int _dataLengthProcessed;
        private int _processIterations = 0;

        /// <summary>
        /// The working data store.
        /// </summary>
        public List<byte> WorkingData { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="FpsCaptureHandler"/>.
        /// </summary>
        public FpsCaptureHandler()
        {
            this.WorkingData = new List<byte>();
            _start = DateTime.Now;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            MMALLog.Logger.LogInformation($"Successfully processed {Helpers.ConvertBytesToMegabytes(_dataLengthProcessed)}.  Process iterations {_processIterations}.");
        }

        /// <inheritdoc />
        public override void Process(ImageContext context)
        {
            _processIterations++;

            this.WorkingData.AddRange(context.Data);
            _dataLengthProcessed += context.Data.Length;
            base.Process(context);

            if (context.Eos)
            {
                if (DateTime.Now - _start > TimeSpan.FromSeconds(1))
                {
                    MMALLog.Logger.LogInformation($"FPS: {_fps}");
                    _fps = 0;
                    _start = DateTime.Now;
                }

                _fps++;
            }
        }

        /// <summary>
        /// Allows us to do any further processing once the capture method has completed. Note: It is the user's responsibility to 
        /// clear the WorkingData list after processing is complete.
        /// </summary>
        public override void PostProcess()
        {
            if (this.OnManipulate != null && this.ImageContext != null)
            {
                this.ImageContext.Data = this.WorkingData.ToArray();
                this.OnManipulate(new FrameProcessingContext(this.ImageContext));
                this.WorkingData = new List<byte>(this.ImageContext.Data);
            }
        }

        /// <inheritdoc />
        public override string TotalProcessed()
        {
            return $"{_dataLengthProcessed}";
        }
    }
}
