using System.Diagnostics;

namespace PowerSchemaFlyout.Services.GameDetectionService.Detectors
{
    public interface IProcessTypeDetector
    {
        ProcessDetectionResult DetectProcessType(Process process);
    }
}
