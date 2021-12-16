using System;
using PowerSchemaFlyout.Services.Detectors;
using PowerSchemaFlyout.Services.Events;

namespace PowerSchemaFlyout.Services
{
    public interface IGameDetectionService
    {
        public void Start(int scanInterval = 5000);
        public void Stop();
        public void RegisterDetector(IProcessTypeDetector processTypeDetector);
        public bool IsRunning();

        event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;
        event EventHandler Started;
        event EventHandler Stopped;
    }
}
