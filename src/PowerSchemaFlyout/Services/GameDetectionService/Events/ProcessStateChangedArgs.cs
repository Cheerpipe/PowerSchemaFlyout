using System;

namespace PowerSchemaFlyout.Services.GameDetectionService.Events
{
    public class ProcessStateChangedArgs : EventArgs
    {
        public ProcessDetectionResult ProcessDetectionResult { get; set; }

        public ProcessStateChangedArgs(ProcessDetectionResult processDetectionResult)
        {
            ProcessDetectionResult = processDetectionResult;
        }

    }
}
