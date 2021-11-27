namespace PowerSchemaFlyout.Services
{
    public interface ISettingsService
    {
        void SetSetting<T>(string settingName, T settingValue);
        T GetSetting<T>(string settingName, T defaultValue);
    }
}
