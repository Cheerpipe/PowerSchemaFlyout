using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    internal class DefaultDetector : BaseProcessTypeDetector
    {

        public override PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            if (currentResult.Preset.ProcessType != ProcessType.Unknown) return currentResult;
            Preset defaultPreset = new Preset(processWatch, processWatch.Title, ConfigurationService.Get().DefaultProfile.ProcessType, ConfigurationService.Get().DefaultProfile.InactiveBackProcessType,
                ConfigurationService.Get().DefaultProfile.InactiveTimeout);
            return new PresetDetectionResult(defaultPreset, processWatch, false, this);

        }
    }
}
