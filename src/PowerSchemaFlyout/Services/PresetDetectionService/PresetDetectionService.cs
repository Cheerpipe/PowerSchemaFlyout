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
        private ProcessWatch _currentForegroundProcessWatch = ProcessWatch.Empty;
        private PresetDetectionResult _currentProcessDetectionResult;

        public PresetDetectionService()
        {
            _processTypeDetectors = new List<IProcessTypeDetector>();
            _multiProcessTypeDetectors = new List<IMultiProcessTypeDetector>();

            _currentProcessDetectionResult = new PresetDetectionResult(Preset.CreateUnknownPreset(), false);
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
                        _currentProcessDetectionResult.Preset.ProcessType == _currentProcessDetectionResult.Preset.InactiveBackProcessType ||
                        _idleState)
                        return;

                    _idleState = true;
                    _idleTimeTimer.Interval = 100;
                    _proactiveScannerTimer.Stop();
                    ProcessStateChanged?.Invoke(this, new ProcessStateChangedArgs(new PresetDetectionResult(Preset.CreateUnknownPreset(_currentProcessDetectionResult.Preset.InactiveBackProcessType), true)));

                }
                else
                {
                    if (!_idleState) return;
                    _idleState = false;
                    _idleTimeTimer.Interval = 500;
                    _proactiveScannerTimer.Start();
                    ProcessStateChanged?.Invoke(this, new ProcessStateChangedArgs(CheckCurrentForegroundProcess(_currentForegroundProcessWatch, true)));

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


                foreach (var md in _multiProcessTypeDetectors)
                {
                    if (md.DetectProcessType(ProcessType.Game))
                    {
                        return new PresetDetectionResult(Preset.CreateGamePreset(_currentForegroundProcessWatch), true);
                    }
                }

                PresetDetectionResult result = new PresetDetectionResult(Preset.CreateUnknownPreset(processWatch), false);

                try
                {
                    foreach (var localResult in _processTypeDetectors.Select(detector => detector.DetectProcessType(processWatch)))
                    {
                        result.ScanIsDefinitive = result.ScanIsDefinitive;
                        result.Preset = (localResult.Preset.ProcessType > result.Preset.ProcessType) ? localResult.Preset : result.Preset;
                        if (result.ScanIsDefinitive || result.Preset.ProcessType != ProcessType.Unknown)
                            break;
                    }
                }
                catch
                {
                    // A lot od things can go wrong
                    // ignored
                }

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
                RaiseAndSetPowerStateChangeIfChanged(new PresetDetectionResult(Preset.CreateUnknownPreset(), false));
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
