using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Enums;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class PresetFileMultiDetector : IMultiProcessTypeDetector, IDisposable
    {
        private List<Preset> _gamesPresets = new List<Preset>();
        private readonly PresetDetectionResult _defaultResult = new PresetDetectionResult(Preset.CreateUnknownPreset(), false);
        private readonly FileSystemWatcher _presetsFileWatcher = new FileSystemWatcher();

        public PresetFileMultiDetector()
        {
            PopulatePresets();

            _presetsFileWatcher.Path = Constants.PresetsFileDirectory;
            _presetsFileWatcher.Changed += _presetsFileWatcher_Changed;
            _presetsFileWatcher.EnableRaisingEvents = true;
        }

        private void _presetsFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            PopulatePresets();
        }

        private void PopulatePresets()
        {
            lock (this)
            {
                _gamesPresets = Kernel.Get<IConfigurationService>().Get().Presets.Where(p => p.ProcessType == ProcessType.Game).ToList(); ;
            }
        }

        public void Dispose()
        {
            _presetsFileWatcher.Dispose();
        }

        public bool DetectProcessType(ProcessType processType)
        {

            foreach (Preset gamePreset in _gamesPresets)
            {
                if (Process.GetProcessesByName("notepad").Length > 0)
                    return true;
            }
            return false;
        }
    }
}
