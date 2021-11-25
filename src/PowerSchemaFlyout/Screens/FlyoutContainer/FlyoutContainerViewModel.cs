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
using PowerSchemaFlyout.Services.CaffeineService;
using PowerSchemaFlyout.Services.GameDetectionService;
using PowerSchemaFlyout.Services.PowerSchemaWatcherService;
using PowerSchemaFlyout.ViewModels;
using ReactiveUI;

namespace PowerSchemaFlyout.Screens.FlyoutContainer
{
    public class FlyoutContainerViewModel : ViewModelBase
    {
        private readonly IFlyoutService _flyoutService;
        private readonly IGameDetectionService _gameDetectionService;
        private readonly ICaffeineService _caffeineService;
        private Timer _backgroundBrushRefreshTimer;

        // Workaround to avoid cyclic redundancy 
        private bool _uiChangeOnly;

        private const int MainPageWidth = 270;
        Win32PowSchemasWrapper pw;

        public FlyoutContainerViewModel(
            IFlyoutService flyoutService,
            IGameDetectionService gameDetectionService,
            IPowerSchemaWatcherService powerSchemaWatcherService,
            ICaffeineService cafeCaffeineService)
        {
            _flyoutService = flyoutService;
            _gameDetectionService = gameDetectionService;
            _caffeineService = cafeCaffeineService;

            powerSchemaWatcherService.PowerPlanChanged += _powerSchemaWatcherService_PowerPlanChanged;

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
            _powerSchemas = new List<PowerSchemaViewModel>();
            foreach (PowerSchema ps in pw.GetCurrentSchemas().ToList())
            {
                _powerSchemas!.Add(new PowerSchemaViewModel(ps.Name, ps.Guid, ps.IsActive));
            }

            _selectedPowerSchema = _powerSchemas!.FirstOrDefault(ps => ps.Guid == pw.GetActiveGuid());
            UpdatePowerSchemasIndicator();
            BackgroundBrush = CreateBackgroundBrush(GetBackgroundBrushColor());
            FlyoutWindowWidth = MainPageWidth;
            FlyoutWindowHeight = _powerSchemas.Count * 52 + 150;
        }

        private void _powerSchemaWatcherService_PowerPlanChanged(object sender, System.EventArgs e)
        {
            _uiChangeOnly = true;
            SelectedPowerSchema = _powerSchemas.FirstOrDefault(ps => ps.Guid == pw.GetActiveGuid());
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
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(35, (byte)(color.R / 1d), (byte)(color.G / 1d), (byte)(color.B / 1d)), 0d));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1d));
            return brush;
        }

        private Color GetBackgroundBrushColor()
        {
            return Colors.Black;

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
                FlyoutWidth = _flyoutWindowWidth;
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
        public bool Caffeine
        {
            get => _caffeineService.IsActive();
            set
            {
                if (value)
                    _caffeineService.Start();
                else
                    _caffeineService.Stop();
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
                FlyoutHeight = _flyoutWindowHeight;
            }
        }

        private double _flyoutHeight;
        public double FlyoutHeight
        {
            get => _flyoutHeight;
            set => this.RaiseAndSetIfChanged(ref _flyoutHeight, value);
        }

        private List<PowerSchemaViewModel> _powerSchemas;
        private PowerSchemaViewModel _selectedPowerSchema;
        public List<PowerSchemaViewModel> PowerSchemas => _powerSchemas;
        public PowerSchemaViewModel SelectedPowerSchema
        {
            get => _selectedPowerSchema;
            set
            {
                if (value == null)
                    return;

                this.RaiseAndSetIfChanged(ref _selectedPowerSchema, value);
                UpdateColorBrush();
                if (!_uiChangeOnly)
                {
                    pw.SetActiveGuid(value.Guid);
                    AutomaticModeEnabled = false;
                }
                _uiChangeOnly = false;
            }
        }

        // TODO: Use settings
        private void UpdatePowerSchemasIndicator()
        {
            foreach (var ps in PowerSchemas)
            {
                if (ps.Guid == PowerSchema.BalancedSchemaGuid)
                    ps.PowerSchemaRol = PowerSchemaRol.Desktop;
                else if (ps.Guid == PowerSchema.MaximumPerformanceSchemaGuid)
                    ps.PowerSchemaRol = PowerSchemaRol.Gaming;
                else
                    ps.PowerSchemaRol = PowerSchemaRol.Unknown;
            }
        }

        public bool AutomaticModeEnabled
        {
            get => _gameDetectionService.IsRunning();
            set
            {
                if (value)
                    _gameDetectionService.Start();
                else
                    _gameDetectionService.Stop();

                UpdateColorBrush();

                this.RaisePropertyChanged(nameof(AutomaticModeEnabled));
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
