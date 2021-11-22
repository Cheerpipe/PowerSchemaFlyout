using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using PowerSchemaFlyout.GameDetection.Enums;

namespace PowerSchemaFlyout.GameDetection.Detectors
{
    internal class GpuLoadDetector : IProcessTypeDetector
    {
        private readonly ManagementScope _scope = new(@"\\" + "." + @"\root\cimv2");

        public GpuLoadDetector()
        {
            _scope.Connect();
        }
        public ProcessDetectionResult DetectProcessType(Process process)
        {
            if (process == null)
                return new ProcessDetectionResult(ProcessType.Unknown, false);

            SelectQuery searchQuery = new SelectQuery(
                $"SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine where name like '%{process.Id}%3D' OR name like '%{process.Id}%Graphics_%'");
            try
            {
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
