using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using PowerSchemaFlyout.Services.GameDetectionService.Detectors;
using PowerSchemaFlyout.Services.GameDetectionService.Enums;
using PowerSchemaFlyout.Services.GameDetectionService.Events;
using PowerSchemaFlyout.Services.GameDetectionService.Native;

namespace PowerSchemaFlyout.Services.GameDetectionService
{
    public class GameDetectionService : IGameDetectionService, IDisposable
    {
        private readonly List<IProcessTypeDetector> _processTypeDetectors;
        private readonly Timer _scannerTimer;
        private readonly Process _currentProcess = Process.GetCurrentProcess();
        private ForegroundWindowWatcher _foregroundWindowWatcher;
        private Process _currentForegroundProcess;
        private ProcessDetectionResult _processDetectionResult;

        public GameDetectionService()
        {
            _processTypeDetectors = new List<IProcessTypeDetector>();
            _scannerTimer = new Timer();
            _scannerTimer.Elapsed += _scannerTimer_Elapsed;
            _processDetectionResult = new ProcessDetectionResult(ProcessType.Unknown, false);
        }

        private void RaiseAndSetPowerStateChangeIfChanged(ProcessDetectionResult processDetectionResult)
        {
            if (_processDetectionResult.ScanIsDefinitive != processDetectionResult.ScanIsDefinitive ||
                _processDetectionResult.ProcessType != processDetectionResult.ProcessType)
            {
                ProcessStateChanged?.Invoke(this, new ProcessStateChangedArgs(processDetectionResult));
            }
            _processDetectionResult = processDetectionResult;
        }

        private void _foregroundWindowWatcher_ForegroundProcessChanged(object sender, ForegroundProcessChangedEventArgs e)
        {
            _currentForegroundProcess?.Dispose();
            _currentForegroundProcess = null;
            _currentForegroundProcess = e.Process;
            _processDetectionResult.Reset();
            RaiseAndSetPowerStateChangeIfChanged(CheckCurrentForegroundProcess(_currentForegroundProcess));
        }

        private void _scannerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RaiseAndSetPowerStateChangeIfChanged(CheckCurrentForegroundProcess(_currentForegroundProcess));
        }

        private ProcessDetectionResult CheckCurrentForegroundProcess(Process process)
        {
            if (_processDetectionResult.ScanIsDefinitive || process?.Id == _currentProcess?.Id || !IsRunning())
                return _processDetectionResult;
            ProcessDetectionResult result = new ProcessDetectionResult(ProcessType.Unknown, false);

            try
            {
                foreach (var localResult in _processTypeDetectors.Select(detector => detector.DetectProcessType(process)))
                {
                    result.ScanIsDefinitive = result.ScanIsDefinitive || localResult.ScanIsDefinitive;
                    result.ProcessType = localResult.ProcessType > result.ProcessType ? localResult.ProcessType : result.ProcessType;
                    if (result.ScanIsDefinitive)
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

        public void RegisterDetector(IProcessTypeDetector processTypeDetector)
        {
            _processTypeDetectors.Add(processTypeDetector);
        }

        public bool IsRunning() => _scannerTimer.Enabled;

        public event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;
        public event EventHandler Started;
        public event EventHandler Stoped;

        public void Start(int scanInterval = 5000)
        {
            if (IsRunning())
                return;

            _scannerTimer.Interval = scanInterval;
            _scannerTimer.Start();
            _foregroundWindowWatcher = new ForegroundWindowWatcher();
            _foregroundWindowWatcher.ForegroundProcessChanged += _foregroundWindowWatcher_ForegroundProcessChanged;
            _foregroundWindowWatcher.Start();
            Started?.Invoke(this, EventArgs.Empty);
            RaiseAndSetPowerStateChangeIfChanged(new ProcessDetectionResult(ProcessType.Unknown, false));
        }
        public void Stop()
        {
            if (!IsRunning())
                return;

            _scannerTimer.Stop();
            _foregroundWindowWatcher.ForegroundProcessChanged -= _foregroundWindowWatcher_ForegroundProcessChanged;
            _foregroundWindowWatcher.Stop();
            _foregroundWindowWatcher = null;
            Stoped?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Stop();
            _scannerTimer?.Dispose();
        }
    }
}
