using System;
using System.Management;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    internal class GpuLoadDetector : BaseProcessTypeDetector
    {
        // ReSharper disable once StringLiteralTypo
        private readonly ManagementScope _scope = new(@"\\" + "." + @"\root\cimv2");

        public GpuLoadDetector()
        {
            _scope.Connect();
        }

        public override PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            if (processWatch.Process == null)
                return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), processWatch, false, this);

            try
            {
                SelectQuery searchQuery = new SelectQuery(
                    $"SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine where name like 'pid_{processWatch.Process.Id}%3D' OR name like '%pid_{processWatch.Process.Id}%Graphics_%'");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_scope, searchQuery);
                ManagementObjectCollection objects = searcher.Get();

                if ((objects.Count == 0))
                {
                    return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), processWatch, false, this);
                }

                foreach (var o in objects)
                {
                    var queryObj = (ManagementObject)o;
                    if ((UInt64)queryObj["UtilizationPercentage"] > (UInt64)ConfigurationService.Get().GpuUsageDetector.Schema)
                    {
                        return new PresetDetectionResult(new Preset(processWatch, processWatch.Title, ProcessType.Game, ProcessType.Game, 0), processWatch, true, this);
                    }
                }
                searcher.Dispose();
                return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), processWatch, false, this);
            }
            catch (ManagementException)
            {
                return new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), processWatch, false, this);
            }
        }
    }
}
