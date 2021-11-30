using System;
using System.Threading;
using System.Windows;
using Accessibility;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.PowerManagement;
using PowerSchemaFlyout.Screens.FlyoutContainer;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.Services.Detectors;
using PowerSchemaFlyout.Services.Enums;
using Application = Avalonia.Application;

namespace PowerSchemaFlyout
{
    public class Program
    {
        public static CancellationTokenSource RunCancellationTokenSource { get; } = new();
        private static readonly CancellationToken RunCancellationToken = RunCancellationTokenSource.Token;

        // This method is needed for IDE previewer infrastructure
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .UseSkia()
                .With(new Win32PlatformOptions()
                {
                    UseWindowsUIComposition = true,
                    CompositionBackdropCornerRadius = 10f,
                });
            return builder;
        }

        // The entry point. Things aren't ready yet, so at this point
        // you shouldn't use any Avalonia types or anything that expects
        // a SynchronizationContext to be ready
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().Start(AppMain, args);
        }

        // Application entry point. Avalonia is completely initialized.
        static void AppMain(Application app, string[] args)
        {
            // Do you startup code here
            Kernel.Initialize(new Bindings());

            IFlyoutService flyoutService = Kernel.Get<IFlyoutService>();
            flyoutService.SetPopulateViewModelFunc(() =>
            {
                return Kernel.Get<FlyoutContainerViewModel>();
            });

            ITrayIconService trayIconService = Kernel.Get<ITrayIconService>();
            IGameDetectionService gameDetectionService = Kernel.Get<IGameDetectionService>();
            IPowerSchemaWatcherService powerSchemaWatcher = Kernel.Get<IPowerSchemaWatcherService>();
            ICaffeineService caffeineService = Kernel.Get<ICaffeineService>();
            ISettingsService settingsService = Kernel.Get<ISettingsService>();
            IPowerManagementServices powerManagementServices = Kernel.Get<IPowerManagementServices>();

            caffeineService.Stop();

            gameDetectionService.Started += (_, _) =>
            {
                lock (powerSchemaWatcher)
                {
                    powerManagementServices.SetActiveGuid(settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")));
                }

            };

            UpdateIcon();

            trayIconService.Show();

            powerSchemaWatcher.StartPlanWatcher();
            powerSchemaWatcher.PowerPlanChanged += (_, _) =>
            {
                UpdateIcon();
            };

            gameDetectionService.ProcessStateChanged += (_, e) =>
            {
                lock (powerSchemaWatcher)
                {
                    switch (e.ProcessDetectionResult.ProcessType)
                    {
                        case ProcessType.GameProcess:
                            powerManagementServices.SetActiveGuid(settingsService.GetSetting("GamingSchemaGuid", new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")));

                            break;
                        case ProcessType.Unknown:
                        case ProcessType.DesktopProcess:
                        default:
                            powerManagementServices.SetActiveGuid(settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")));
                            break;
                    }
                }
            };
            gameDetectionService.RegisterDetector(new BlackListDetector());
            gameDetectionService.RegisterDetector(new WhiteListDetector());
            gameDetectionService.RegisterDetector(new GpuLoadDetector());

            if (settingsService.GetSetting("AutomaticMode", true) || settingsService.GetSetting("EnableAutomaticModeOnStartup", true))
            {
                gameDetectionService.Start();
            }

            // Start the main loop
            app.Run(RunCancellationToken);

            // Stop things
            trayIconService.Hide();
            powerSchemaWatcher.StopPlanWatcher();
            gameDetectionService.Stop();
        }

        public static void UpdateIcon()
        {
            ITrayIconService trayIconService = Kernel.Get<ITrayIconService>();
            IPowerSchemaWatcherService powerSchemaWatcher = Kernel.Get<IPowerSchemaWatcherService>();
            ISettingsService settingsService = Kernel.Get<ISettingsService>();
            IPowerManagementServices powerManagementServices = Kernel.Get<IPowerManagementServices>();

            lock (powerSchemaWatcher)
            {
                Guid newSchema = powerManagementServices.GetActiveGuid();

                if (newSchema == settingsService.GetSetting("GamingSchemaGuid", new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")))
                {
                    trayIconService.UpdateIcon("power_gaming.ico");
                }
                else if (newSchema == settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")))
                {
                    trayIconService.UpdateIcon("power_balanced.ico");
                }
                else if (newSchema == settingsService.GetSetting("PowerSchemaSaverGuid", new Guid("a1841308-3541-4fab-bc81-f71556f20b4a")))
                {
                    trayIconService.UpdateIcon("power_saver.ico");
                }
                else
                {
                    trayIconService.UpdateIcon("power_automatic_mode_off.ico");
                }
            }
        }
    }
}
