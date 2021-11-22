using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PowerSchemaFlyout.GameDetection.Enums;

namespace PowerSchemaFlyout.GameDetection.Detectors
{
    public class WhiteListDetector : IProcessTypeDetector
    {
        private List<string> _whiteList;

        public WhiteListDetector()
        {
            _whiteList = System.IO.File.ReadAllLines("whitelist").ToList();
        }
        public ProcessDetectionResult DetectProcessType(Process process)
        {
            if (process == null)
                return new ProcessDetectionResult(ProcessType.Unknown, false);

            foreach (string wlPath in _whiteList)
            {
                if (process.MainModule!.FileName!.ToLower().Contains(wlPath))
                {
                    return new ProcessDetectionResult(ProcessType.GameProcess, true);
                }
            }
            return new ProcessDetectionResult(ProcessType.DesktopProcess, false);
        }
    }
}
