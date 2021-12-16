using System.IO;

namespace PowerSchemaFlyout
{
    public class Constants
    {
        public static readonly string PresetsFileDirectory = Directory.GetCurrentDirectory();
        public static readonly string PresetsFileName = "presets.json";
        public static readonly string PresetsFilePath = Path.Combine(PresetsFileDirectory, PresetsFileName);
        public static readonly int AutomaticPowerSaveModeTimeout = 5000;
    }
}
