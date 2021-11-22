using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using PowerSchemaFlyout.Models;
using PowerSchemaFlyout.PowerManagement;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.ViewModels;
using ReactiveUI;

namespace PowerSchemaFlyout.Screens
{
    public class FlyoutContainerViewModel : ViewModelBase
    {
        private readonly IFlyoutService _flyoutService;
        private Timer _backgroundBrushRefreshTimer;

        //private const int MainPageHeight = 530;
        private const int MainPageWidth = 290;
        Win32PowSchemasWrapper pw;

        public FlyoutContainerViewModel(IFlyoutService flyoutService)
        {
            _flyoutService = flyoutService;

            this.WhenActivated(disposables =>
            {
                /* Handle activation */
                Disposable
                    .Create(() =>
                    {
                        /* Handle deactivation */
                        _backgroundBrushRefreshTimer?.DisposeAsync();
                        _backgroundBrushRefreshTimer = null;
                    })
                    .DisposeWith(disposables);
            });

            pw = new Win32PowSchemasWrapper();

            _powerSchemas = pw.GetCurrentSchemas().ToList();


            _selectedPowerSchema = _powerSchemas.FirstOrDefault(ps => ps.Guid == pw.GetActiveGuid());
            BackgroundBrush = CreateBackgroundBrush(GetBackgroundBrushColor());
            FlyoutWindowWidth = MainPageWidth;
            FlyoutWindowHeight = _powerSchemas.Count * 52 + 90;
            AutomaticModeEnabled=Program.AutomaticModeEnabled;      

        }

        private void UpdateColorBrush()
        {
            Color color = GetBackgroundBrushColor();
            Dispatcher.UIThread.Post(() => { BackgroundBrush = CreateBackgroundBrush(color); });
        }

        private LinearGradientBrush CreateBackgroundBrush(Color color)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative)
            };
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(50, (byte)(color.R/2), (byte)(color.G / 2), (byte)(color.B / 2)), 0d));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1d));
            return brush;
        }

        private Color GetBackgroundBrushColor()
        {
            if (_selectedPowerSchema.Guid == PowerSchema.PowerSchemaSaver || 
                _selectedPowerSchema.Name!.ToLower().Contains("econom") ||
                _selectedPowerSchema.Name.ToLower().Contains("saver"))
                return Colors.Green;
            else if (_selectedPowerSchema.Guid == PowerSchema.BalancedSchemaGuid ||
                _selectedPowerSchema.Name.ToLower().Contains("balanc"))
                return Colors.Yellow;
            else if (_selectedPowerSchema.Guid == PowerSchema.MaximumPerformanceSchemaGuid || 
                _selectedPowerSchema.Name.ToLower().Contains("rend") ||
                _selectedPowerSchema.Name.ToLower().Contains("perf") ||
                _selectedPowerSchema.Name.ToLower().Contains("ultimat") ||
                _selectedPowerSchema.Name.ToLower().Contains("razer") ||
                _selectedPowerSchema.Name.ToLower().Contains("cortex") ||
                _selectedPowerSchema.Name.ToLower().Contains("max") ||
                _selectedPowerSchema.Name.ToLower().Contains("gam"))
                return Colors.Red;
            else if (_selectedPowerSchema.Name.ToLower().Contains("samsung"))
                return Colors.Blue;
            else
                return Colors.Transparent;
        }

        private double _flyoutWindowWidth;
        public double FlyoutWindowWidth
        {
            get => _flyoutWindowWidth;
            set
            {
                _flyoutService.SetWidth(value);
                _flyoutWindowWidth = value;
                FlyoutWidth = _flyoutWindowWidth - FlyoutSpacing;
            }
        }

        private LinearGradientBrush _backgroundBrush;
        public LinearGradientBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                _backgroundBrush = value;
                this.RaisePropertyChanged(nameof(BackgroundBrush));
            }
        }

        private double _flyoutWidth;
        public double FlyoutWidth
        {
            get => _flyoutWidth;
            set => this.RaiseAndSetIfChanged(ref _flyoutWidth, value);
        }


        private double _flyoutWindowHeight;
        public double FlyoutWindowHeight
        {
            get => _flyoutWindowHeight;
            set
            {
                _flyoutService.SetHeight(value);
                _flyoutWindowHeight = value;
                FlyoutHeight = _flyoutWindowHeight - FlyoutSpacing;
            }
        }

        private double _flyoutHeight;
        public double FlyoutHeight
        {
            get => _flyoutHeight;
            set => this.RaiseAndSetIfChanged(ref _flyoutHeight, value);
        }

        public int FlyoutSpacing => 12;

        private List<PowerSchema> _powerSchemas;
        private PowerSchema _selectedPowerSchema;

        public List<PowerSchema> PowerSchemas => _powerSchemas;
        public PowerSchema SelectedPowerSchema
        {
            get => _selectedPowerSchema;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPowerSchema, value);
                pw.SetActiveGuid(value.Guid);
                UpdateColorBrush();
                AutomaticModeEnabled = false;
            }
        }

        private bool _automaticModeEnabled;
        public bool AutomaticModeEnabled
        {
            get => _automaticModeEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _automaticModeEnabled, value);
                Program.AutomaticModeEnabled = value;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void GoSettingsCommand()
        {
            Windows.Run("cmd", " /k start powercfg.cpl && exit");
        }

    }
    public class Windows
    {
        public static void Run(string commandLine, string arguments = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = commandLine;
            startInfo.Arguments = arguments;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden; //Hides GUI
            startInfo.CreateNoWindow = true; //Hides console
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
