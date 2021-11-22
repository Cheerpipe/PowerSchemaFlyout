using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PowerSchemaFlyout.GameDetection;
using PowerSchemaFlyout.GameDetection.Detectors;
using PowerSchemaFlyout.GameDetection.Enums;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.PowerManagement;
using PowerSchemaFlyout.Services;

namespace PowerSchemaFlyout
{
    public class Program
    {
        public static CancellationTokenSource RunCancellationTokenSource { get; } = new();
        private static readonly Win32PowSchemasWrapper Win32PowSchemasWrapper = new();
        internal static bool AutomaticModeEnabled = true;

        private static readonly CancellationToken RunCancellationToken = RunCancellationTokenSource.Token;

        // This method is needed for IDE previewer infrastructure
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
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

            GameDetectionService gs = new GameDetectionService(5000);
            gs.ProcessStateChanged += Gs_ProcessStateChanged;
            gs.RegisterDetector(new BlackListDetector());
            gs.RegisterDetector(new WhiteListDetector());
            gs.RegisterDetector(new GpuLoadDetector());
            gs.Start();

            var trayIconService = Kernel.Get<ITrayIconService>();
            trayIconService.Show();
            trayIconService.UpdateIcon("Flyout.ico");

            // Start the main loop
            app.Run(RunCancellationToken);

            // Stop things
            trayIconService.Hide();
            gs.Stop();
            gs.Dispose();
        }

        private static void Gs_ProcessStateChanged(object sender, GameDetection.Events.ProcessStateChangedArgs e)
        {
            var trayIconService = Kernel.Get<ITrayIconService>();
            if (!AutomaticModeEnabled)
            {
                trayIconService.UpdateIcon("power_automatic_mode_off.ico");
                trayIconService.UpdateTooltip("Power mode: Manual");
                return;
            }

            switch (e.ProcessDetectionResult.ProcessType)
            {
                case ProcessType.GameProcess:
                    Win32PowSchemasWrapper.SetActiveGuid(Guid.Parse("0ba05a3e-884a-4278-b5be-59b313ea8d48"));
                    trayIconService.UpdateIcon("power_gaming.ico");
                    trayIconService.UpdateTooltip("Power mode: Gaming");
                    break;
                default:
                    Win32PowSchemasWrapper.SetActiveGuid(Guid.Parse("381b4222-f694-41f0-9685-ff5bb260df2e"));
                    trayIconService.UpdateIcon("power_saver.ico");
                    trayIconService.UpdateTooltip("Power mode: Desktop");
                    break;
            }
        }
    }
}
