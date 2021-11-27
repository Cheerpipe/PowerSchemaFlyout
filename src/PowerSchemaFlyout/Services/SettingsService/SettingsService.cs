using System;
using System.IO;
using System.Threading;
using NoSqlRepositories.Core;
using NoSqlRepositories.JsonFiles;

namespace PowerSchemaFlyout.Services
{
    public class Settings : IBaseEntity
    {
        public string SettingName { get; set; }
        public object SettingValue { get; set; }
        public string Id { get; set; }
        public DateTimeOffset SystemCreationDate { get; set; }
        public DateTimeOffset SystemLastUpdateDate { get; set; }
        public bool Deleted { get; set; }
    }

    public class SettingsService : ISettingsService
    {
        private readonly JsonFileRepository<Settings> _jsonFileRepository = new JsonFileRepository<Settings>(Directory.GetCurrentDirectory(), "settings");

        public void SetSetting<T>(string settingName, T settingValue)
        {
            lock (this)
            {
                Settings setting = new Settings() { SettingName = settingName, SettingValue = settingValue, Id = settingName };
                _jsonFileRepository.Update(setting);
            }
        }

        public T GetSetting<T>(string settingName, T defaultValue)
        {
            lock (this)
            {
                Settings setting = _jsonFileRepository.TryGetById(settingName);

                if (setting != null)
                {
                    if (setting.SettingValue is T)
                    {
                        return (T)setting.SettingValue;
                    }
                    // Specific types that may need specific conversions
                    else if (typeof(T) == typeof(Guid))
                    {
                        // Ugly, I know, but works
                        return (T)(object)Guid.Parse((string)setting.SettingValue);
                    }
                }
                SetSetting(settingName, defaultValue);
                return defaultValue;
            }
        }
    }
}
