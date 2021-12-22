using System.Diagnostics;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class CpuUsageDetector : IProcessTypeDetector
    {
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public PresetDetectionResult DetectProcessType(ProcessWatch process)
        {
            float cpuUsage = _cpuCounter.NextValue();
            
            //TODO: Make this configurable from json file
            if (cpuUsage > 50f) //TODO: Make this a param.
                return new PresetDetectionResult(
                    new Preset(process, process.ProcessName, ProcessType.DesktopMedium, ProcessType.DesktopMedium, 0), // Just medium for now. If there is a relative high cpu load it means cpu should work fast.
                    false);
            return new PresetDetectionResult(Preset.CreateUnknownPreset(), false);
        }
    }
}
