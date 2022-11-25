using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using FluentAvalonia.Styling;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Screens.FlyoutContainer;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Detectors;
using PowerSchemaFlyout.Services.Enums;
using Serilog;
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

            IConfigurationService configurationService = Kernel.Get<IConfigurationService>();
            configurationService.Load();

            ITrayIconService trayIconService = Kernel.Get<ITrayIconService>();
            IPresetDetectionService presetDetectionService = Kernel.Get<IPresetDetectionService>();
            IPowerSchemaWatcherService powerSchemaWatcher = Kernel.Get<IPowerSchemaWatcherService>();
            ICaffeineService caffeineService = Kernel.Get<ICaffeineService>();
            ISettingsService settingsService = Kernel.Get<ISettingsService>();
            IPowerManagementServices powerManagementServices = Kernel.Get<IPowerManagementServices>();
            ILogger logger = Kernel.Get<ILogger>();


            caffeineService.Stop();

            presetDetectionService.Started += (_, _) =>
            {
                lock (powerSchemaWatcher)
                {
                    powerManagementServices.SetActiveGuid(settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")));
                }
            };

            UpdateIcon();

            var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

            //TODO: Theme service
            thm.RequestedTheme = "Dark";

            trayIconService.Show();

            powerSchemaWatcher.StartPlanWatcher();
            powerSchemaWatcher.PowerPlanChanged += (_, _) =>
            {
                UpdateIcon();
            };

            presetDetectionService.ProcessStateChanged += (_, e) =>
            {
                logger.Verbose($"Setting power mode {e.ProcessDetectionResult.Preset.ProcessType} for {e.ProcessDetectionResult.ProcessWatch.ProcessName}");
                switch (e.ProcessDetectionResult.Preset.ProcessType)
                {
                    case ProcessType.DesktopLow:
                        powerManagementServices.SetActiveGuid(settingsService.GetSetting("PowerSaverGuid", new Guid("a1841308-3541-4fab-bc81-f71556f20b4a")));
                        break;
                    case ProcessType.DesktopMedium:
                        powerManagementServices.SetActiveGuid(settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")));
                        break;
                    case ProcessType.DesktopHigh:
                        powerManagementServices.SetActiveGuid(settingsService.GetSetting("GamingSchemaGuid", new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")));
                        break;
                    case ProcessType.Game:
                        powerManagementServices.SetActiveGuid(settingsService.GetSetting("GamingSchemaGuid", new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")));
                        break;
                    case ProcessType.Unknown:
                        powerManagementServices.SetActiveGuid(settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")));
                        break;
                    default:
                        powerManagementServices.SetActiveGuid(settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")));
                        break;
                }
            };

            // Multi detectors. This detector will override all other detectors
            presetDetectionService.RegisterMultiDetector(new PresetFileMultiDetector());

            // This detectors will be executed in the same order as registered
            presetDetectionService.RegisterDetector(new CpuUsageDetector());    //Will set value if result is higher than current result
            presetDetectionService.RegisterDetector(new PresetFileDetector());  //Will set value if result is higher than current result //This is a definitive detector. This means if there is a match here detectors below won't be used.
            presetDetectionService.RegisterDetector(new GpuLoadDetector());     //Will set a game value and will act as a definitive if match.
            presetDetectionService.RegisterDetector(new DefaultDetector());     //Will set value if current result is unknown.

            if (settingsService.GetSetting("AutomaticMode", true) || settingsService.GetSetting("EnableAutomaticModeOnStartup", true))
            {
                presetDetectionService.Start();
            }

            // Start the main loop
            logger.Information("Power Schema Flyout started");
            app.Run(RunCancellationToken);
            logger.Information("Power Schema Flyout stopped");
            // Stop things
            trayIconService.Hide();
            powerSchemaWatcher.StopPlanWatcher();
            presetDetectionService.Stop();
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
