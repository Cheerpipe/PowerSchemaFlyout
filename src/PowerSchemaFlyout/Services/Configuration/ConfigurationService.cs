using System.IO;
using Newtonsoft.Json;
using PowerSchemaFlyout.Models.Configuration;

namespace PowerSchemaFlyout.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private Configurations? _configurations;
        public ConfigurationService()
        {
            Load();
        }

        public void Load()
        {
            using StreamReader r = new StreamReader(Constants.PresetsFilePath);
            string appsettingsString = r.ReadToEnd();
            _configurations = JsonConvert.DeserializeObject<Configurations>(appsettingsString);
        }

        public Configurations Get() => _configurations!;
    }
}
