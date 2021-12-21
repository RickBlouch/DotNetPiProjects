namespace MotionCapture.Application.Device
{
    public interface IDeviceManager
    {
        Task WaitForInitialize();

        void DisableLed(LedColor color);

        void EnableLed(LedColor color);

        void ToggleLed(LedColor color, bool enable);

        void RegisterForButtonPressCallback(Func<CancellationToken, Task> func);  // TODO: ValueTask?
    }
}
