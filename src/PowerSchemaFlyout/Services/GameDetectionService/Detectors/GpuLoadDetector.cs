using System;
using System.Diagnostics;
using System.Management;
using PowerSchemaFlyout.Services.Enums;

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
        public ProcessDetectionResult DetectProcessType(Process process)
        {
            if (process == null)
                return new ProcessDetectionResult(ProcessType.Unknown, false);

            try
            {
                SelectQuery searchQuery = new SelectQuery(
                    $"SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine where name like '%{process.Id}%3D' OR name like '%{process.Id}%Graphics_%'");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_scope, searchQuery);
                ManagementObjectCollection objects = searcher.Get();

                if ((objects.Count == 0))
                {
                    return new ProcessDetectionResult(ProcessType.DesktopProcess, false);
                }

                foreach (var o in objects)
                {
                    var queryObj = (ManagementObject)o;
                    if ((UInt64)queryObj["UtilizationPercentage"] > 25)
                    {
                        return new ProcessDetectionResult(ProcessType.GameProcess, false);
                    }
                }
                searcher.Dispose();
                return new ProcessDetectionResult(ProcessType.DesktopProcess, false);
            }
            catch (ManagementException)
            {
                return new ProcessDetectionResult(ProcessType.DesktopProcess, false);
            }
        }
    }
}
