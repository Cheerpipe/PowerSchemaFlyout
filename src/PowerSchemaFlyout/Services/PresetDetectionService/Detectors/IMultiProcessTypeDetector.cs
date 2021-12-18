using System.Collections.Generic;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public interface IMultiProcessTypeDetector
    {
        bool DetectProcessType(ProcessType processType);
    }
}
