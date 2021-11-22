using System;
using PowerSchemaFlyout.GameDetection.Detectors;
using PowerSchemaFlyout.GameDetection.Events;

namespace PowerSchemaFlyout.GameDetection
{
    public interface IGameDetectionService
    {
        public void Start();
        public void Stop();
        public void RegisterDetector(IProcessTypeDetector processTypeDetector);

        event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;
    }
}
