using MMALSharp.Handlers;

namespace MotionCapture.Application.Device.Mmal
{
    public class CustomImageStreamCaptureHandler : FileStreamCaptureHandler
    {

        private int _fileNumber = 0;

        public CustomImageStreamCaptureHandler(string fullPath)
            : base(fullPath)
        {

        }

        public override void NewFile()
        {
            if (CurrentStream == null)
            {
                return;
            }

            CurrentStream?.Dispose();

            _fileNumber++;
            var newFilename = $"{Directory}/{CurrentFilename} {_fileNumber.ToString("D2")}.{Extension}";
            
            CurrentStream = File.Create(newFilename);
        }

    }
}
