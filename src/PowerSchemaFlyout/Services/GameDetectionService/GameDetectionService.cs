using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using PowerSchemaFlyout.Services.Detectors;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Events;
using PowerSchemaFlyout.Services.Native;
using Timer = System.Timers.Timer;

namespace PowerSchemaFlyout.Services
{
    public class GameDetectionService : IGameDetectionService, IDisposable
    {
        private readonly List<IProcessTypeDetector> _processTypeDetectors;
        private readonly Timer _scannerTimer;
        private readonly Process _currentProcess = Process.GetCurrentProcess();
        private ForegroundWindowWatcher _foregroundWindowWatcher;
        private ProcessWatch _currentForegroundProcessWatch;
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
                //Debug.WriteLine("aaa");
            }
            _processDetectionResult = processDetectionResult;
        }

        private void _foregroundWindowWatcher_ForegroundProcessChanged(object sender, ProcessWatch e)
        {
            if (_currentForegroundProcessWatch != null)
            {
                _currentForegroundProcessWatch.Process?.Dispose();
                _currentForegroundProcessWatch.Process = null;
            }
            _currentForegroundProcessWatch = e;
            _processDetectionResult.Reset();
            RaiseAndSetPowerStateChangeIfChanged(CheckCurrentForegroundProcess(_currentForegroundProcessWatch));
        }

        private void _scannerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RaiseAndSetPowerStateChangeIfChanged(CheckCurrentForegroundProcess(_currentForegroundProcessWatch));
        }

        private ProcessDetectionResult CheckCurrentForegroundProcess(ProcessWatch processWatch)
        {
            lock (this)
            {
                if (processWatch == null)
                    return new ProcessDetectionResult(ProcessType.Unknown, false);

                if (_processDetectionResult.ScanIsDefinitive || processWatch.Process?.Id == _currentProcess?.Id || !IsRunning())
                    return _processDetectionResult;


                ProcessDetectionResult result = new ProcessDetectionResult(ProcessType.Unknown, false);

                try
                {
                    foreach (var localResult in _processTypeDetectors.Select(detector => detector.DetectProcessType(processWatch)))
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

            lock (this)
            {
                _scannerTimer.Interval = scanInterval;
                _scannerTimer.Start();
                _foregroundWindowWatcher = new ForegroundWindowWatcher();
                _foregroundWindowWatcher.ForegroundProcessChanged += _foregroundWindowWatcher_ForegroundProcessChanged;
                _foregroundWindowWatcher.Start();
                Started?.Invoke(this, EventArgs.Empty);
                RaiseAndSetPowerStateChangeIfChanged(new ProcessDetectionResult(ProcessType.Unknown, false));
            }
        }
        public void Stop()
        {
            if (!IsRunning())
                return;

            lock (this)
            {
                _scannerTimer.Stop();
                _foregroundWindowWatcher.ForegroundProcessChanged -= _foregroundWindowWatcher_ForegroundProcessChanged;
                _foregroundWindowWatcher.Stop();
                Stoped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            Stop();
            _scannerTimer?.Dispose();

            foreach (var d in _processTypeDetectors)
            {
                if (d is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
}
