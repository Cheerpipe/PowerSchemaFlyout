using System;
using System.Management;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    internal class GpuLoadDetector : IProcessTypeDetector
    {
        // ReSharper disable once StringLiteralTypo
        private readonly ManagementScope _scope = new(@"\\" + "." + @"\root\cimv2");
        private readonly IConfigurationService _configurationService;

        public GpuLoadDetector()
        {
            _configurationService = Kernel.Get<IConfigurationService>();
            _scope.Connect();
        }

        public PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            if (processWatch.Process == null)
                return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), false);

            try
            {
                SelectQuery searchQuery = new SelectQuery(
                    $"SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine where name like 'pid_{processWatch.Process.Id}%3D' OR name like '%pid_{processWatch.Process.Id}%Graphics_%'");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_scope, searchQuery);
                ManagementObjectCollection objects = searcher.Get();

                if ((objects.Count == 0))
                {
                    return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), false);
                }

                foreach (var o in objects)
                {
                    var queryObj = (ManagementObject)o;
                    if ((UInt64)queryObj["UtilizationPercentage"] > (UInt64)_configurationService.Get().GpuUsageDetector.Schema)
                    {
                        return new PresetDetectionResult(new Preset(processWatch, processWatch.Title, ProcessType.Game, ProcessType.Game, 0), true);
                    }
                }
                searcher.Dispose();
                return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), false);
            }
            catch (ManagementException)
            {
                return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), false);
            }
        }
    }
}
