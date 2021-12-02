using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Media;
using PowerSchemaFlyout.Models;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.ViewModels;
using ReactiveUI;

namespace PowerSchemaFlyout.Screens.FlyoutContainer
{
    public class FlyoutContainerViewModel : ViewModelBase
    {
        private readonly IFlyoutService _flyoutService;
        private readonly IGameDetectionService _gameDetectionService;
        private readonly ICaffeineService _caffeineService;
        private readonly ISettingsService _settingsService;

        // Workaround to avoid cyclic redundancy 
        private bool _ignoreChange;

        private const int MainPageWidth = 270;
        private readonly IPowerManagementServices _powerManagementServices;

        public FlyoutContainerViewModel(
            IFlyoutService flyoutService,
            IGameDetectionService gameDetectionService,
            IPowerSchemaWatcherService powerSchemaWatcherService,
            ICaffeineService cafeCaffeineService,
            ISettingsService settingsService,
            IPowerManagementServices powerManagementServices)
        {
            _flyoutService = flyoutService;
            _gameDetectionService = gameDetectionService;
            _caffeineService = cafeCaffeineService;
            _settingsService = settingsService;
            _powerManagementServices = powerManagementServices;

            this.WhenActivated(disposables =>
            {
                /* Handle activation */
                powerSchemaWatcherService.PowerPlanChanged += PowerSchemaWatcherService_PowerPlanChanged;
                Disposable
                    .Create(() =>
                    {
                        /* Handle deactivation */
                        // _defferedDisableAutomaticModeTimer?.Dispose();
                        // _defferedDisableAutomaticModeTimer = null;
                        powerSchemaWatcherService.PowerPlanChanged -= PowerSchemaWatcherService_PowerPlanChanged;

                    })
                    .DisposeWith(disposables);
            });

            _powerSchemas = new List<PowerSchemaViewModel>();
            foreach (PowerSchema ps in _powerManagementServices.GetCurrentSchemas().ToList())
            {
                _powerSchemas!.Add(new PowerSchemaViewModel(ps.Name, ps.Guid, ps.IsActive));
            }

            _selectedPowerSchema = _powerSchemas!.FirstOrDefault(ps => ps.Guid == _powerManagementServices.GetActiveGuid());
            UpdatePowerSchemasIndicator();
            FlyoutWindowWidth = MainPageWidth;
            FlyoutWindowHeight = _powerSchemas.Count * 52 + 150;
        }

        private void PowerSchemaWatcherService_PowerPlanChanged(object sender, EventArgs e)
        {
            lock (this)
            {
                _ignoreChange = true;
                SelectedPowerSchema = _powerSchemas.FirstOrDefault(ps => ps.Guid == _powerManagementServices.GetActiveGuid());
            }
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
                lock (this)
                {
                    if (value == null)
                        return;

                    _powerManagementServices.SetActiveGuid(value.Guid);
                    this.RaiseAndSetIfChanged(ref _selectedPowerSchema, value);

                    if (!_ignoreChange)
                        AutomaticModeEnabled = false;
                    _ignoreChange = false;
                }
            }
        }

        public bool AutomaticModeEnabled
        {
            get => _gameDetectionService.IsRunning();
            set
            {
                lock (this)
                {
                    if (value)
                        _gameDetectionService.Start();
                    else
                        _gameDetectionService.Stop();

                    _settingsService.SetSetting("AutomaticMode", value);
                    this.RaisePropertyChanged(nameof(AutomaticModeEnabled));
                }
            }
        }

        private void UpdatePowerSchemasIndicator()
        {
            foreach (var ps in PowerSchemas)
            {
                if (ps.Guid == _settingsService.GetSetting("BalancedSchemaGuid", new Guid("381b4222-f694-41f0-9685-ff5bb260df2e")))
                    ps.PowerSchemaRol = PowerSchemaRol.Desktop;
                else if (ps.Guid == _settingsService.GetSetting("GamingSchemaGuid", new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")))
                    ps.PowerSchemaRol = PowerSchemaRol.Gaming;
                else if (ps.Guid == _settingsService.GetSetting("PowerSchemaSaverGuid", new Guid("a1841308-3541-4fab-bc81-f71556f20b4a")))
                    ps.PowerSchemaRol = PowerSchemaRol.PowerSaving;
                else
                    ps.PowerSchemaRol = PowerSchemaRol.Unknown;
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
