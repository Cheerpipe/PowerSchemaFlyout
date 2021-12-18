using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using PowerSchemaFlyout.Models.Configuration;

namespace PowerSchemaFlyout.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private static readonly object FileAccessLock = new ();
        private Configurations? _configurations;

        public void Load()
        {
            lock (FileAccessLock)
            {
                Thread.Sleep(100);
                using Stream s = new FileStream(Constants.PresetsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new StreamReader(s, Encoding.UTF8);
                string appsettingsString = sr.ReadToEnd();
                _configurations = JsonConvert.DeserializeObject<Configurations>(appsettingsString) ?? throw new InvalidOperationException("Presets file read failed.");
            }
        }

        public Configurations Get()
        {
            lock (FileAccessLock)
            {
                return _configurations!;
            }
        }
    }
}