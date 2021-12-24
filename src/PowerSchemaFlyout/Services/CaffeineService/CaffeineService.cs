using PowerSchemaFlyout.Platform.Windows;

namespace PowerSchemaFlyout.Services
{
    public class CaffeineService : ICaffeineService
    {
        private bool _state;
        public void Start()
        {
            PreventSleep();
            _state = true;
        }

        public void Stop()
        {
            PreventSleep();
            _state = false;
        }

        private static void PreventSleep()
        {
            NativeMethods.SetThreadExecutionState(ExecutionState.EsContinuous | ExecutionState.EsSystemRequired);
        }

        private static void AllowSleep()
        {

            NativeMethods.SetThreadExecutionState(ExecutionState.EsContinuous);
        }

        public bool IsActive()
        {
            return _state;
        }
    }
}
