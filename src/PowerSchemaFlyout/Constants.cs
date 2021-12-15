using System.IO;

namespace PowerSchemaFlyout
{
    public class Constants
    {
        public static readonly string PresetsFileDirectory = Directory.GetCurrentDirectory();
        public static readonly string PresetsFileName = "presets.txt";
        public static readonly string PresetsFilePath = Path.Combine(PresetsFileDirectory, PresetsFileName);
    }
}
