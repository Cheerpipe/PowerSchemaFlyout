using System;

namespace PowerSchemaFlyout.Services
{
    public interface IPowerSchemaWatcherService
    {
        void StartPlanWatcher();
        void StopPlanWatcher();
        event EventHandler PowerPlanChanged;
    }
}
