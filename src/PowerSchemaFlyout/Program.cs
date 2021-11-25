using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models;
using PowerSchemaFlyout.PowerManagement;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.Services.CaffeineService;
using PowerSchemaFlyout.Services.GameDetectionService;
using PowerSchemaFlyout.Services.GameDetectionService.Detectors;
using PowerSchemaFlyout.Services.GameDetectionService.Enums;
using PowerSchemaFlyout.Services.PowerSchemaWatcherService;

namespace PowerSchemaFlyout
{
    public class Program
    {
        public static CancellationTokenSource RunCancellationTokenSource { get; } = new();
        private static readonly Win32PowSchemasWrapper Win32PowSchemasWrapper = new();
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

            ITrayIconService trayIconService = Kernel.Get<ITrayIconService>();
            IGameDetectionService gameDetectionService = Kernel.Get<IGameDetectionService>();
            IPowerSchemaWatcherService powerSchemaWatcher = Kernel.Get<IPowerSchemaWatcherService>();
            ICaffeineService caffeineService = Kernel.Get<ICaffeineService>();

            caffeineService.Stop();

            gameDetectionService.Started += (_, _) =>
            {
                lock (powerSchemaWatcher)
                {
                    Win32PowSchemasWrapper.SetActiveGuid(PowerSchema.MaximumPerformanceSchemaGuid);
                    Win32PowSchemasWrapper.SetActiveGuid(PowerSchema.BalancedSchemaGuid);
                }
            };



            trayIconService.Show();
            trayIconService.UpdateIcon("Flyout.ico");

            powerSchemaWatcher.StartPlanWatcher();
            powerSchemaWatcher.PowerPlanChanged += (_, _) =>
            {
                lock (powerSchemaWatcher)
                {
                    Guid newSchema = Win32PowSchemasWrapper.GetActiveGuid();

                    if (newSchema == PowerSchema.MaximumPerformanceSchemaGuid)
                    {
                        trayIconService.UpdateIcon("power_gaming.ico");
                    }
                    else if (newSchema == PowerSchema.BalancedSchemaGuid)
                    {
                        trayIconService.UpdateIcon("power_balanced.ico");
                    }
                    else if (newSchema == PowerSchema.PowerSchemaSaver)
                    {
                        trayIconService.UpdateIcon("power_saver.ico");
                    }
                    else
                    {
                        trayIconService.UpdateIcon("power_automatic_mode_off.ico");
                    }
                }
            };

            gameDetectionService.ProcessStateChanged += (_, e) =>
            {
                lock (powerSchemaWatcher)
                {
                    switch (e.ProcessDetectionResult.ProcessType)
                    {
                        case ProcessType.GameProcess:
                            Win32PowSchemasWrapper.SetActiveGuid(PowerSchema.MaximumPerformanceSchemaGuid);

                            break;
                        case ProcessType.Unknown:
                        case ProcessType.DesktopProcess:
                        default:
                            Win32PowSchemasWrapper.SetActiveGuid(PowerSchema.BalancedSchemaGuid);
                            break;
                    }
                }
            };
            gameDetectionService.RegisterDetector(new BlackListDetector());
            gameDetectionService.RegisterDetector(new WhiteListDetector());
            gameDetectionService.RegisterDetector(new GpuLoadDetector());
            gameDetectionService.Start();

            // Start the main loop
            app.Run(RunCancellationToken);

            // Stop things
            trayIconService.Hide();
            powerSchemaWatcher.StopPlanWatcher();
            gameDetectionService.Stop();
        }
    }
}
