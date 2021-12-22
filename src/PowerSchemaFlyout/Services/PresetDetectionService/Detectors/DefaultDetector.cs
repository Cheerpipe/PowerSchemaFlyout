﻿using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    internal class DefaultDetector : IProcessTypeDetector
    {
        public PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            if (currentResult.Preset.ProcessType != ProcessType.Unknown) return currentResult;
            Preset defaultPreset = new Preset(processWatch, processWatch.Title, ProcessType.DesktopMedium, ProcessType.DesktopLow,
                3000);
            return new PresetDetectionResult(defaultPreset, false);

        }
    }
}
