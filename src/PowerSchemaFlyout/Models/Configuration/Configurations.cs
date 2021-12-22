using System.Collections.Generic;

namespace PowerSchemaFlyout.Models.Configuration
{

    public class Configurations
    {
        public List<Preset> Presets { get; set; }
        public CpuUsageDetector CpuUsageDetector { get; set; }
        public GpuUsageDetector GpuUsageDetector { get; set; }
        public DefaultProfile DefaultProfile { get; set; }
    }
}
