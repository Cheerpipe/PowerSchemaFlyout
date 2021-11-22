using PowerSchemaFlyout.Services.GameDetectionService.Enums;

namespace PowerSchemaFlyout.Services.GameDetectionService
{
    public class ProcessDetectionResult
    {
        public ProcessType ProcessType { get; set; }
        public bool ScanIsDefinitive { get; set; }

        public ProcessDetectionResult(ProcessType processType, bool scanIsDefinitive)
        {
            ProcessType = processType;
            ScanIsDefinitive = scanIsDefinitive;
        }

        public ProcessDetectionResult Reset()
        {
            ProcessType = ProcessType.Unknown;
            ScanIsDefinitive = false;
            return this;
        }
    }
}
