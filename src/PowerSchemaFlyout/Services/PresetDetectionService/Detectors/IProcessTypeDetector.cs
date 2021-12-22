using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public interface IProcessTypeDetector
    {
        PresetDetectionResult DetectProcessType(ProcessWatch process, PresetDetectionResult currentResult);
    }
}
