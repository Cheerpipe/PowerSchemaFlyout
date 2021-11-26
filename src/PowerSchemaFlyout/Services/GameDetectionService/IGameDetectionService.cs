using System;
using PowerSchemaFlyout.Services.GameDetectionService.Detectors;
using PowerSchemaFlyout.Services.GameDetectionService.Events;

namespace PowerSchemaFlyout.Services.GameDetectionService
{
    public interface IGameDetectionService
    {
        public void Start(int scanInterval = 5000);
        public void Stop();
        public void RegisterDetector(IProcessTypeDetector processTypeDetector);
        public bool IsRunning();

        event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;
        event EventHandler Started;
        event EventHandler Stoped;
    }
}
