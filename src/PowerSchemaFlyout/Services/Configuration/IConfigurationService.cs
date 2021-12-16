using PowerSchemaFlyout.Models.Configuration;

namespace PowerSchemaFlyout.Services.Configuration
{
    public interface IConfigurationService
    {
        Configurations Get();
        void Load();
    }
}
