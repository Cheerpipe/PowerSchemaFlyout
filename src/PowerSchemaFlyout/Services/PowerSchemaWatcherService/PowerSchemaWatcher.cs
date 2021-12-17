using System;
using System.Globalization;
using System.Management;

namespace PowerSchemaFlyout.Services
{
    public class PowerSchemaWatcherService : IPowerSchemaWatcherService, IDisposable
    {
        private const string ActivePlanKeyName = @"SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes";
        private const string ActivePlanKeyValueName = "ActivePowerScheme";
        private ManagementEventWatcher _currentPowerPlanWatcher;
        public event EventHandler PowerPlanChanged;

        public void StartPlanWatcher()
        {
            string currentPowerPlanWatcherQuery = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_LOCAL_MACHINE' AND KeyPath = '{0}' AND ValueName = '{1}'",
                ActivePlanKeyName,
                ActivePlanKeyValueName);

            _currentPowerPlanWatcher = new ManagementEventWatcher(currentPowerPlanWatcherQuery);
            _currentPowerPlanWatcher.EventArrived += CurrentPowerPlanWatcher_EventArrived;
            _currentPowerPlanWatcher.Start();
        }

        public void StopPlanWatcher()
        {
            _currentPowerPlanWatcher.Stop();
            _currentPowerPlanWatcher.Dispose();
        }

        private void CurrentPowerPlanWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            PowerPlanChanged.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            StopPlanWatcher();
            _currentPowerPlanWatcher.Dispose();
        }
    }
}
