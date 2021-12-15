using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public interface IProcessTypeDetector
    {
        ProcessDetectionResult DetectProcessType(ProcessWatch process);
    }
}
