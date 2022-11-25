using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services
{
    public class PresetDetectionResult
    {
        public Preset Preset { get; set; }
        public ProcessWatch ProcessWatch { get; set; }
        public bool ScanIsDefinitive { get; set; }
        public string DetectorName { get; set; }

        public PresetDetectionResult(Preset preset, ProcessWatch processWatch, bool scanIsDefinitive, object detector)
        {
            Preset = preset;
            ScanIsDefinitive = scanIsDefinitive;
            ProcessWatch = processWatch;
            DetectorName = detector.GetType().FullName;
        }

        public PresetDetectionResult Reset()
        {
            Preset = Preset.CreateUnknownPreset();
            ProcessWatch = ProcessWatch.Empty;
            ScanIsDefinitive = false;
            return this;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
