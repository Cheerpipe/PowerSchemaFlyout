using System;

namespace PowerSchemaFlyout.Services.Events
{
    public class ProcessStateChangedArgs : EventArgs
    {
        public PresetDetectionResult ProcessDetectionResult { get; set; }

        public ProcessStateChangedArgs(PresetDetectionResult processDetectionResult)
        {
            ProcessDetectionResult = processDetectionResult;
        }

    }
}
