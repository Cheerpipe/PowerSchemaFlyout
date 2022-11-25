using System.Diagnostics;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;
using PowerSchemaFlyout.Utiles;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class CpuUsageDetector : BaseProcessTypeDetector
    {
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public override PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            float cpuUsage = _cpuCounter.NextValue();

            ProcessType returnType = IComparableUtiles.Max(currentResult.Preset.ProcessType, ConfigurationService.Get().CpuUsageDetector.Schema);

            if (cpuUsage > ConfigurationService.Get().CpuUsageDetector.CpuUsageThreshold)
                return new PresetDetectionResult(
                    new Preset(processWatch, processWatch.ProcessName, returnType, returnType, int.MaxValue), processWatch, false, this);
            return new PresetDetectionResult(Preset.CreateUnknownPreset(), processWatch, false, this);
        }
    }
}
