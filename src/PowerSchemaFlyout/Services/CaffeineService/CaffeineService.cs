namespace PowerSchemaFlyout.Services
{
    public class CaffeineService : ICaffeineService
    {
        private bool _state;
        public void Start()
        {
            NativeMethods.PreventSleep();
            _state = true;
        }

        public void Stop()
        {
            NativeMethods.PreventSleep();
            _state = false;
        }

        public bool IsActive()
        {
            return _state;
        }
    }
}
