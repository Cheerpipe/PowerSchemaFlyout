namespace PowerSchemaFlyout.Services
{
    public interface ICaffeineService
    {
        void Start();
        void Stop();
        bool IsActive();
    }
}
