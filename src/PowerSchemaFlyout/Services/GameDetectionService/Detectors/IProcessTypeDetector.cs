using System.Diagnostics;

namespace PowerSchemaFlyout.Services.Detectors
{
    public interface IProcessTypeDetector
    {
        ProcessDetectionResult DetectProcessType(Process process);
    }
}
