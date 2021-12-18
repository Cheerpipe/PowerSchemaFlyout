using System.IO;
using System.Text;
using Newtonsoft.Json;
using PowerSchemaFlyout.Models.Configuration;

namespace PowerSchemaFlyout.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private static readonly object _fileAccessLock = new object();
        private Configurations? _configurations;
        public ConfigurationService()
        {
            Load();
        }

        public void Load()
        {
            lock (_fileAccessLock)
            {
                using Stream s = new FileStream(Constants.PresetsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new StreamReader(s, Encoding.UTF8);
                string appsettingsString = sr.ReadToEnd();
                _configurations = JsonConvert.DeserializeObject<Configurations>(appsettingsString);
            }
        }

        public Configurations Get() => _configurations!;
    }
}
