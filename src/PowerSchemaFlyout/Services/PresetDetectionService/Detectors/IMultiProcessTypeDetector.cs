using PowerSchemaFlyout.Services.Enums;

namespace PowerSchemaFlyout.Services.Detectors
{
    public interface IMultiProcessTypeDetector
    {
        bool DetectProcessType(ProcessType processType);
    }
}
