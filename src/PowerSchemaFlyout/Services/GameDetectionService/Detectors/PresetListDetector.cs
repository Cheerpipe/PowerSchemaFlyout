﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class PresetListDetector : IProcessTypeDetector, IDisposable
    {
        private List<Preset> _presets = new List<Preset>();
        private readonly ProcessDetectionResult DefaultResult = new ProcessDetectionResult(ProcessType.Desktop, false);
        private readonly FileSystemWatcher _presetsFileWatcher = new FileSystemWatcher();

        public PresetListDetector()
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
                _presets.Clear();
                List<string> lines = new List<string>();
                using FileStream fs = new FileStream(Constants.PresetsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new StreamReader(fs);
                List<String> lst = new List<string>();
                while (!sr.EndOfStream)
                    lines.Add(sr.ReadLine());


                foreach (string s in lines)
                {
                    if (s.StartsWith(';'))
                        continue;

                    string[] sPreset = s.Split(':');
                    if (sPreset.Length < 2 || sPreset.Length > 3)
                        continue;

                    Preset preset = new Preset();

                    string path = sPreset[0].ToLower();
                    ProcessType processType = sPreset[1] switch
                    {
                        "1" => ProcessType.LowDemand,
                        "2" => ProcessType.Desktop,
                        "3" => ProcessType.Game,
                        _ => ProcessType.Desktop
                    };

                    preset.Path = path;
                    preset.ProcessType = processType;

                    if (sPreset.Length == 3)
                    {
                        preset.Title = sPreset[2];
                    }
                    _presets.Add(preset);
                }
            }
        }

        public ProcessDetectionResult DetectProcessType(ProcessWatch processWatch)
        {
            if (processWatch.Process == null)
                return new ProcessDetectionResult(ProcessType.Unknown, false);

            lock (this)
            {
                // Get presets for process
                List<Preset> _processPresets = _presets.Where(preset => processWatch.FileName.Contains(preset.Path)).ToList();

                // Return default (Desktop) mode if there are no profiles.
                if (_processPresets.Count == 0)
                    return DefaultResult;

                //First, Check if process match any preset with title
                if (_processPresets.FirstOrDefault(preset => processWatch.FileName.Contains(preset.Path) && processWatch.Title == preset.Title) is Preset thePresetWithTitle)
                    return new ProcessDetectionResult(thePresetWithTitle.ProcessType, true);

                //Second, Check if process match any preset without title
                if (_processPresets.FirstOrDefault(preset => processWatch.FileName.ToLower().Contains(preset.Path) && preset.Title == null) is Preset thePresetWithoutTitle)
                    return new ProcessDetectionResult(thePresetWithoutTitle.ProcessType, true);

                // If dectection don't dive any resutls, result default result
                return DefaultResult;
            }
        }

        public void Dispose()
        {

        }
    }

    public class Preset
    {
        public string Path { get; set; }
        public ProcessType ProcessType { get; set; }
        public string? Title { get; set; }
    }
}
