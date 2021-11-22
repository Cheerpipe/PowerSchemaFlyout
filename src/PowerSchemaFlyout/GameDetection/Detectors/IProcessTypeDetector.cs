using System.Diagnostics;
using PowerSchemaFlyout.GameDetection.Enums;

namespace PowerSchemaFlyout.GameDetection.Detectors
{
    public interface IProcessTypeDetector
    {
        ProcessDetectionResult DetectProcessType(Process process);
    }
}
