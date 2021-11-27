using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PowerSchemaFlyout.Services.Enums;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class BlackListDetector : IProcessTypeDetector
    {
        private List<string> _blackList;

        public BlackListDetector()
        {
            _blackList = System.IO.File.ReadAllLines("blacklist").ToList();
        }
        public ProcessDetectionResult DetectProcessType(Process process)
        {
            if (process == null)
                return new ProcessDetectionResult(ProcessType.Unknown, false);

            if (_blackList.Any(wlPath => process.MainModule!.FileName!.ToLower().Contains(wlPath)))
            {
                return new ProcessDetectionResult(ProcessType.DesktopProcess, true);
            }
            return new ProcessDetectionResult(ProcessType.DesktopProcess, false);
        }
    }
}