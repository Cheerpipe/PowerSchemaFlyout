using System;
using System.Management;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    internal class GpuLoadDetector : IProcessTypeDetector
    {
        // ReSharper disable once StringLiteralTypo
        private readonly ManagementScope _scope = new(@"\\" + "." + @"\root\cimv2");

        public GpuLoadDetector()
        {
            _scope.Connect();
        }
        public ProcessDetectionResult DetectProcessType(ProcessWatch processWatch)
        {
            if (processWatch.Process == null)
                return new ProcessDetectionResult(ProcessType.Unknown, false);

            try
            {
                SelectQuery searchQuery = new SelectQuery(
                    $"SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine where name like 'pid_{processWatch.Process.Id}%3D' OR name like '%pid_{processWatch.Process.Id}%Graphics_%'");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_scope, searchQuery);
                ManagementObjectCollection objects = searcher.Get();

                if ((objects.Count == 0))
                {
                    return new ProcessDetectionResult(ProcessType.Desktop, false);
                }

                foreach (var o in objects)
                {
                    var queryObj = (ManagementObject)o;
                    if ((UInt64)queryObj["UtilizationPercentage"] > 25)
                    {
                        return new ProcessDetectionResult(ProcessType.Game, true);
                    }
                }
                searcher.Dispose();
                return new ProcessDetectionResult(ProcessType.Desktop, false);
            }
            catch (ManagementException)
            {
                return new ProcessDetectionResult(ProcessType.Desktop, false);
            }
        }
    }
}
