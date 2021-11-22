using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using PowerSchemaFlyout.GameDetection.Detectors;
using PowerSchemaFlyout.GameDetection.Enums;
using PowerSchemaFlyout.GameDetection.Events;
using PowerSchemaFlyout.GameDetection.Native;

namespace PowerSchemaFlyout.GameDetection
{
    public class GameDetectionService : IGameDetectionService, IDisposable
    {
        private readonly List<IProcessTypeDetector> _processTypeDetectors;
        private readonly Timer _scannerTimer;
        private readonly Process _currentProcess = Process.GetCurrentProcess();
        private ForegroundWindowWatcher _foregroundWindowWatcher;
        private Process _currentForegroundProcess;
        private ProcessDetectionResult _processDetectionResult;

        public GameDetectionService(int scanInterval = 1000)
        {
            _processTypeDetectors = new List<IProcessTypeDetector>();
            _scannerTimer = new Timer(scanInterval);
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
            if (_processDetectionResult.ScanIsDefinitive || process.Id == _currentProcess.Id)
                return _processDetectionResult;

            ProcessDetectionResult result = new ProcessDetectionResult(ProcessType.Unknown, false);
            foreach (IProcessTypeDetector detector in _processTypeDetectors)
            {
                ProcessDetectionResult localResult = detector.DetectProcessType(process);
                result.ScanIsDefinitive = result.ScanIsDefinitive || localResult.ScanIsDefinitive;
                result.ProcessType = localResult.ProcessType > result.ProcessType ? localResult.ProcessType : result.ProcessType;
                if (result.ScanIsDefinitive)
                    break;
            }
            return result;
        }

        public void RegisterDetector(IProcessTypeDetector processTypeDetector)
        {
            _processTypeDetectors.Add(processTypeDetector);
        }

        public event EventHandler<ProcessStateChangedArgs> ProcessStateChanged;

        public void Start()
        {
            _scannerTimer.Start();

            _foregroundWindowWatcher = new ForegroundWindowWatcher();
            _foregroundWindowWatcher.ForegroundProcessChanged += _foregroundWindowWatcher_ForegroundProcessChanged;
            _foregroundWindowWatcher.Start();
        }
        public void Stop()
        {
            _scannerTimer.Stop();

            _foregroundWindowWatcher.ForegroundProcessChanged -= _foregroundWindowWatcher_ForegroundProcessChanged;
            _foregroundWindowWatcher = null;
        }

        public void Dispose()
        {
            Stop();
            _scannerTimer?.Dispose();
        }
    }
}
