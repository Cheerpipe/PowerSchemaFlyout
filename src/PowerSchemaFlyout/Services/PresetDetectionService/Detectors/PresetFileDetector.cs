using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Native;
using PowerSchemaFlyout.Utiles;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class PresetFileDetector : BaseProcessTypeDetector, IDisposable
    {
        private List<Preset> _presets = new List<Preset>();
        private readonly PresetDetectionResult _defaultResult;
        private readonly FileSystemWatcher _presetsFileWatcher = new FileSystemWatcher();

        public PresetFileDetector() : base()
        {
            _defaultResult = new PresetDetectionResult(Preset.CreateUnknownPreset(), ProcessWatch.Empty, false, this);
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
            ConfigurationService.Load();
            _presets = ConfigurationService.Get().Presets;
            _presets.ForEach(p =>
            {
                p.ProcessName = p.ProcessName.Trim().ToLower();
                if (p.Title != null) p.Title = p.Title.ToLower().Trim();
            });
        }

        public override PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            if (processWatch.Process == null)
                return new PresetDetectionResult(Preset.CreateUnknownPreset(), processWatch, false, this);

            lock (this)
            {
                // Get presets for process
                List<Preset> processPresets = _presets.Where(preset => processWatch.ProcessName == preset.ProcessName).ToList();

                // Return default (Desktop) mode if there are no profiles.
                if (processPresets.Count == 0)
                {
                    // To detect applications without preset.
                    lock (Application.Current)
                    {
                        File.AppendAllText("withoutpreset.txt",
                            $"{processWatch.ProcessName} - {processWatch.Title}" + Environment.NewLine);
                    }

                    return _defaultResult;
                }

                //First, Check if process match any preset with title
                if (processPresets.FirstOrDefault(preset => processWatch.ProcessName == preset.ProcessName && String.Equals(processWatch.Title, preset.Title, StringComparison.CurrentCultureIgnoreCase)) is { } thePresetWithTitle)
                {
                    Preset resultPresetWithTitle = thePresetWithTitle.Clone();
                    resultPresetWithTitle.ProcessType = IComparableUtiles.Max(resultPresetWithTitle.ProcessType, currentResult.Preset.ProcessType);
                    return new PresetDetectionResult(resultPresetWithTitle, processWatch, true, this);
                }

                //Second, Check if process match any preset without title
                if (processPresets.FirstOrDefault(preset => processWatch.ProcessName == preset.ProcessName && preset.Title == null) is { } thePresetWithoutTitle)
                {
                    Preset resultPresetWithoutTitle = thePresetWithoutTitle.Clone();
                    resultPresetWithoutTitle.ProcessType = IComparableUtiles.Max(resultPresetWithoutTitle.ProcessType, currentResult.Preset.ProcessType);
                    return new PresetDetectionResult(resultPresetWithoutTitle, processWatch, true, this);
                }

                // If detection don't dive any results, result default result
                // To detect applications without preset.
                lock (Application.Current)
                {
                    File.AppendAllText("withoutpreset.txt",
                        $"{processWatch.ProcessName} - {processWatch.Title}" + Environment.NewLine);
                }

                // If detection don't dive any results, result default result
                return _defaultResult;
            }
        }

        public void Dispose()
        {
            _presetsFileWatcher.EnableRaisingEvents = false;
            _presetsFileWatcher.Dispose();
        }
    }
}
