using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    public class PresetFileDetector : IProcessTypeDetector, IDisposable
    {
        private List<Preset> _presets = new List<Preset>();
        private readonly PresetDetectionResult _defaultResult = new PresetDetectionResult(Preset.CreateUnknownPreset(), false);
        private readonly FileSystemWatcher _presetsFileWatcher = new FileSystemWatcher();

        public PresetFileDetector()
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
            Kernel.Get<IConfigurationService>().Load();
            _presets = Kernel.Get<IConfigurationService>().Get().Presets;
            _presets.ForEach(p =>
            {
                p.ProcessName = p.ProcessName.Trim().ToLower();
                if (p.Title != null) p.Title = p.Title.ToLower().Trim();
            });
        }

        public PresetDetectionResult DetectProcessType(ProcessWatch processWatch)
        {
            if (processWatch.Process == null)
                return new PresetDetectionResult(Preset.CreateUnknownPreset(), false);

            lock (this)
            {
                // Get presets for process
                List<Preset> processPresets = _presets.Where(preset => processWatch.ProcessName == preset.ProcessName).ToList();

                // Return default (Desktop) mode if there are no profiles.
                if (processPresets.Count == 0)
                {
                    // To detect applications without preset.
                    // TODO: Proper IO write - Unique values - Clear values with presets
                    lock (Application.Current)
                    {
                        File.AppendAllText("withoutpreset.txt",
                            $"{processWatch.ProcessName} - {processWatch.Title}" + Environment.NewLine);
                    }

                    return _defaultResult;
                }

                //First, Check if process match any preset with title
                if (processPresets.FirstOrDefault(preset => processWatch.ProcessName == preset.ProcessName && String.Equals(processWatch.Title, preset.Title, StringComparison.CurrentCultureIgnoreCase)) is { } thePresetWithTitle)
                    return new PresetDetectionResult(thePresetWithTitle, true);

                //Second, Check if process match any preset without title
                if (processPresets.FirstOrDefault(preset => processWatch.ProcessName == preset.ProcessName && preset.Title == null) is { } thePresetWithoutTitle)
                    return new PresetDetectionResult(thePresetWithoutTitle, true);


                // If dectection don't dive any results, result default result
                // To detect applications without preset.
                // TODO: Proper IO write - Unique values - Clear values with presets
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
            _presetsFileWatcher.Dispose();
        }
    }
}
