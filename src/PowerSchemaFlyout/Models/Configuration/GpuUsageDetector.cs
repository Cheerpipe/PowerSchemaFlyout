using PowerSchemaFlyout.Services.Enums;

namespace PowerSchemaFlyout.Models.Configuration
{
    public class GpuUsageDetector
    {
        public float GpuUsageThreshold { get; set; }
        public ProcessType Schema { get; set; }
    }
}
