﻿using System;
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
        private readonly FileSystemWatcher _presetsFileWatcher = new FileSystemWatcher();
        private readonly IConfigurationService _configurationService;

        public PresetFileMultiDetector()
        {
            _configurationService = Kernel.Get<IConfigurationService>();
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
            _configurationService.Load();
            _gamesPresets = _configurationService.Get().Presets.Where(p => p.ProcessType == ProcessType.Game).ToList();
            _gamesPresets.ForEach(p =>
            {
                p.ProcessName = p.ProcessName.Trim().ToLower();
                if (p.Title != null) p.Title = p.Title.ToLower().Trim();
            });
        }

        public void Dispose()
        {
            _presetsFileWatcher.Dispose();
        }

        public bool DetectProcessType(ProcessType processType)
        {

            foreach (Preset gamePreset in _gamesPresets)
            {
                if (Process.GetProcessesByName(gamePreset.ProcessName).Length > 0)
                    return true;
            }
            return false;
        }
    }
}
