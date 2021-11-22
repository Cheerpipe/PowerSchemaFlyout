using System;
using PowerSchemaFlyout.Services.GameDetectionService.Detectors;
using PowerSchemaFlyout.Services.GameDetectionService.Events;

namespace PowerSchemaFlyout.Services.GameDetectionService
{
    public interface IGameDetectionService
    {
        public void Start();
        public void Stop();
        public void RegisterDetector(IProcessTypeDetector processTypeDetector);
        public bool IsRunning();

        event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;
        event EventHandler Started;
        event EventHandler Stoped;
    }
}
