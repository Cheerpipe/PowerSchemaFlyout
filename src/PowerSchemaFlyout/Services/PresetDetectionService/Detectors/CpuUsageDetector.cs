using System.Diagnostics;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class CpuUsageDetector : IProcessTypeDetector
    {
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private readonly IConfigurationService _configurationService;
        public CpuUsageDetector()
        {
            _configurationService = Kernel.Get<IConfigurationService>();
        }

        public PresetDetectionResult DetectProcessType(ProcessWatch process)
        {
            float cpuUsage = _cpuCounter.NextValue();
            if (cpuUsage > _configurationService.Get().CpuUsageDetector.CpuUsageThreshold)
                return new PresetDetectionResult(
                    new Preset(process, process.ProcessName, _configurationService.Get().CpuUsageDetector.Schema, _configurationService.Get().CpuUsageDetector.Schema, 0),
                    false);
            return new PresetDetectionResult(Preset.CreateUnknownPreset(), false);
        }
    }
}
