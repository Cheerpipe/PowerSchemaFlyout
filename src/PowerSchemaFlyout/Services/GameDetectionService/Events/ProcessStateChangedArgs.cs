using System;

namespace PowerSchemaFlyout.Services.Events
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
