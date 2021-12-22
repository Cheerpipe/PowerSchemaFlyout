using System.Diagnostics;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;
using PowerSchemaFlyout.Utiles;

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

        public PresetDetectionResult DetectProcessType(ProcessWatch process, PresetDetectionResult currentResult)
        {
            float cpuUsage = _cpuCounter.NextValue();

            ProcessType returnType = IComparableUtiles.Max(currentResult.Preset.ProcessType, _configurationService.Get().CpuUsageDetector.Schema);

            if (cpuUsage > _configurationService.Get().CpuUsageDetector.CpuUsageThreshold)
                return new PresetDetectionResult(
                    new Preset(process, process.ProcessName, returnType, returnType, 0),
                    false);
            return new PresetDetectionResult(Preset.CreateUnknownPreset(), false);
        }
    }
}
