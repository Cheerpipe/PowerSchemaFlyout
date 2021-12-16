using PowerSchemaFlyout.Models.Configuration;

namespace PowerSchemaFlyout.Services
{
    public class PresetDetectionResult
    {
        public Preset Preset { get; set; }
        public bool ScanIsDefinitive { get; set; }

        public PresetDetectionResult(Preset preset, bool scanIsDefinitive)
        {
            Preset = preset;
            ScanIsDefinitive = scanIsDefinitive;
        }

        public PresetDetectionResult Reset()
        {
            Preset = Preset.CreateUnknownPreset();
            ScanIsDefinitive = false;
            return this;
        }
    }
}
