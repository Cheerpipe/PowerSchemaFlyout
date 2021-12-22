using PowerSchemaFlyout.Services.Enums;

namespace PowerSchemaFlyout.Models.Configuration
{
    public class CpuUsageDetector
    {
        public float CpuUsageThreshold { get; set; }
        public ProcessType Schema { get; set; }
    }
}
