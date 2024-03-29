﻿using System;
using PowerSchemaFlyout.Services.Detectors;
using PowerSchemaFlyout.Services.Events;

namespace PowerSchemaFlyout.Services
{
    public interface IPresetDetectionService
    {
        public void Start(int scanInterval = 5000);
        public void Stop();
        public void RegisterDetector(IProcessTypeDetector processTypeDetector);
        public void RegisterMultiDetector(IMultiProcessTypeDetector processTypeDetector);
        public bool IsRunning();

        event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;
        event EventHandler Started;
        event EventHandler Stopped;
    }
}
