using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Detectors;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Events;
using PowerSchemaFlyout.Services.Native;
using Serilog;

namespace PowerSchemaFlyout.Services
{
    public class PresetDetectionService : IPresetDetectionService, IDisposable
    {

        private readonly List<IProcessTypeDetector> _processTypeDetectors;
        private readonly List<IMultiProcessTypeDetector> _multiProcessTypeDetectors;

        private readonly Timer _proactiveScannerTimer;
        private readonly Timer _idleTimeTimer;
        private readonly Process _thisProcess = Process.GetCurrentProcess();
        private readonly ForegroundWindowWatcher _foregroundWindowWatcher;
        private readonly ILogger _logger;
        private ProcessWatch _currentForegroundProcessWatch = ProcessWatch.Empty;
        private PresetDetectionResult _currentProcessDetectionResult;


        public PresetDetectionService(ILogger logger)
        {
            _logger = logger;
            _processTypeDetectors = new List<IProcessTypeDetector>();
            _multiProcessTypeDetectors = new List<IMultiProcessTypeDetector>();

            _currentProcessDetectionResult = new PresetDetectionResult(Preset.CreateUnknownPreset(), ProcessWatch.Empty, false, this);
            _foregroundWindowWatcher = new ForegroundWindowWatcher();

            _proactiveScannerTimer = new Timer();
            _proactiveScannerTimer.Elapsed += _proactiveScannerTimer_Elapsed;

            _idleTimeTimer = new Timer();
            _idleTimeTimer.Elapsed += _idleTimeTimer_Elapsed;
        }

        private bool _idleState;
        private void _idleTimeTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                if (IdleTimeFinder.GetIdleTime() > _currentProcessDetectionResult.Preset.InactiveTimeout)
                {
                    // This apply only for non games processes. Also ignore if process is already in power save mode
                    if (_currentProcessDetectionResult.Preset.ProcessType is ProcessType.Game or ProcessType.DesktopLow ||
                        _currentProcessDetectionResult.Preset.InactiveTimeout == 0 ||
                        _currentProcessDetectionResult.Preset.InactiveTimeout == int.MaxValue ||
                        _currentProcessDetectionResult.Preset.ProcessType == _currentProcessDetectionResult.Preset.InactiveBackProcessType ||
                        _idleState)
                        return;

                    _idleState = true;
                    _idleTimeTimer.Interval = 100;
                    _proactiveScannerTimer.Stop();
                    ProcessStateChanged?.Invoke(this, new ProcessStateChangedArgs(new PresetDetectionResult(Preset.CreateUnknownPreset(_currentProcessDetectionResult.Preset.InactiveBackProcessType), _currentProcessDetectionResult.ProcessWatch, true, this)));
                    _logger.Verbose($"Going into idle state for {_currentProcessDetectionResult.ProcessWatch.ProcessName}");
                }
                else
                {
                    if (!_idleState)
                        return;
                    _idleState = false;
                    _idleTimeTimer.Interval = 500;
                    _proactiveScannerTimer.Start();
                    ProcessStateChanged?.Invoke(this, new ProcessStateChangedArgs(CheckCurrentForegroundProcess(_currentForegroundProcessWatch, true)));
                    _logger.Verbose($"Exiting from idle state with {_currentProcessDetectionResult.ProcessWatch.ProcessName}");
                }
            }
        }

        private void RaiseAndSetPowerStateChangeIfChanged(PresetDetectionResult processDetectionResult)
        {
            lock (this)
            {
                if (_currentProcessDetectionResult.ScanIsDefinitive == processDetectionResult.ScanIsDefinitive &&
                    _currentProcessDetectionResult.Preset.ProcessType ==
                    processDetectionResult.Preset.ProcessType) return;

                _currentProcessDetectionResult = processDetectionResult;
                ProcessStateChanged?.Invoke(this, new ProcessStateChangedArgs(processDetectionResult));
            }
        }

        //TODO: _currentProcessDetectionResult name and title are resulting in an empty fields
        private void _foregroundWindowWatcher_ForegroundProcessChanged(object sender, ProcessWatch e)
        {
            lock (this)
            {
                _currentForegroundProcessWatch.Process?.Dispose();
                _currentForegroundProcessWatch.Process = null;
                _currentForegroundProcessWatch = e;
                _currentProcessDetectionResult.Reset();
                RaiseAndSetPowerStateChangeIfChanged(CheckCurrentForegroundProcess(_currentForegroundProcessWatch));
            }
        }

        private void _proactiveScannerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                RaiseAndSetPowerStateChangeIfChanged(CheckCurrentForegroundProcess(_currentForegroundProcessWatch));
            }
        }

        private PresetDetectionResult CheckCurrentForegroundProcess(ProcessWatch processWatch, bool force = false)
        {
            lock (this)
            {
                if ((_currentProcessDetectionResult.ScanIsDefinitive || processWatch.Process?.Id == _thisProcess.Id || !IsRunning()) && !force)
                    return _currentProcessDetectionResult;



                foreach (var backgroundProcesses in _multiProcessTypeDetectors.Select(multioMultiProcessTypeDetector => multioMultiProcessTypeDetector.DetectProcessType(ProcessType.Game)).Where(backgroundProcesses => backgroundProcesses.Count > 0))
                {
                    return new PresetDetectionResult(Preset.CreateGamePreset(backgroundProcesses.First()), backgroundProcesses.First(), true, this); //TODO: Generalize and use returned process
                }

                PresetDetectionResult result = new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), processWatch, false, this);

                try
                {
                    foreach (var localResult in _processTypeDetectors.Select(detector => detector.DetectProcessType(processWatch, result)))
                    {

                        result.ScanIsDefinitive = localResult.ScanIsDefinitive;

                        //Copy preset and apply some logic
                        result.Preset.ProcessType = localResult.Preset.ProcessType;
                        result.Preset.Title = localResult.Preset.Title;
                        result.Preset.ProcessName = localResult.Preset.ProcessName;
                        result.Preset.InactiveBackProcessType = localResult.Preset.InactiveBackProcessType;
                        result.Preset.InactiveTimeout = Math.Max(localResult.Preset.InactiveTimeout, result.Preset.InactiveTimeout);
                        result.DetectorName= localResult.DetectorName;

                        if (!result.ScanIsDefinitive) continue;
                        break;
                    }
                }
                catch
                {
                    // A lot od things can go wrong
                    // ignored
                }

                _logger.Verbose($"Detection result for {result.Preset.ProcessName} is {result.Preset.ProcessType} using detector {result.DetectorName}. Definitive: {result.ScanIsDefinitive}");

                return result;
            }
        }

        public void RegisterDetector(IProcessTypeDetector processTypeDetector)
        {
            _processTypeDetectors.Add(processTypeDetector);
        }

        public void RegisterMultiDetector(IMultiProcessTypeDetector processTypeDetector)
        {
            _multiProcessTypeDetectors.Add(processTypeDetector);
        }

        public bool IsRunning() => _proactiveScannerTimer.Enabled;

        public event EventHandler<ProcessStateChangedArgs>? ProcessStateChanged;
        public event EventHandler? Started;
        public event EventHandler? Stopped;

        public void Start(int scanInterval = 5000)
        {
            if (IsRunning())
                return;

            lock (this)
            {
                _proactiveScannerTimer.Interval = scanInterval;
                _proactiveScannerTimer.Start();

                _idleTimeTimer.Interval = 500;
                _idleTimeTimer.Start();

                _foregroundWindowWatcher.ForegroundProcessChanged += _foregroundWindowWatcher_ForegroundProcessChanged;
                _foregroundWindowWatcher.Start();
                Started?.Invoke(this, EventArgs.Empty);
                RaiseAndSetPowerStateChangeIfChanged(new PresetDetectionResult(Preset.CreateUnknownPreset(), ProcessWatch.Empty, false, this)); // Is this necessary?
                _logger.Information("Preset detection service started");
            }
        }
        public void Stop()
        {
            if (!IsRunning())
                return;

            lock (this)
            {
                _proactiveScannerTimer.Stop();
                _idleTimeTimer.Stop();

                _foregroundWindowWatcher.ForegroundProcessChanged -= _foregroundWindowWatcher_ForegroundProcessChanged;
                _foregroundWindowWatcher.Stop();
                Stopped?.Invoke(this, EventArgs.Empty);
                _logger.Information("Preset detection service stopped");
            }
        }

        public void Dispose()
        {
            Stop();
            _proactiveScannerTimer.Dispose();
            _idleTimeTimer.Dispose();

            foreach (var d in _processTypeDetectors)
            {
                if (d is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
}
