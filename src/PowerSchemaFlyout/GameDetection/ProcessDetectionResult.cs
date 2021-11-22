using PowerSchemaFlyout.GameDetection.Enums;

namespace PowerSchemaFlyout.GameDetection
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
