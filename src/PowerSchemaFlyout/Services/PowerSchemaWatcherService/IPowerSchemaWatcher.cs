using System;

namespace PowerSchemaFlyout.Services.PowerSchemaWatcherService
{
    public interface IPowerSchemaWatcherService
    {
        void StartPlanWatcher();
        void StopPlanWatcher();
        event EventHandler PowerPlanChanged;
    }
}
