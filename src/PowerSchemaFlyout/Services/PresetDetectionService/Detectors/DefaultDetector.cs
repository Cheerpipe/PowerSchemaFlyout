using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Models.Configuration;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Services.Detectors
{
    internal class DefaultDetector : IProcessTypeDetector
    {
        private readonly IConfigurationService _configurationService;

        public DefaultDetector()
        {
            _configurationService = Kernel.Get<IConfigurationService>();
        }

        public PresetDetectionResult DetectProcessType(ProcessWatch processWatch, PresetDetectionResult currentResult)
        {
            if (currentResult.Preset.ProcessType != ProcessType.Unknown) return currentResult;
            Preset defaultPreset = new Preset(processWatch, processWatch.Title, _configurationService.Get().DefaultProfile.InactiveBackProcessType, _configurationService.Get().DefaultProfile.InactiveBackProcessType,
                _configurationService.Get().DefaultProfile.InactiveTimeout);
            return new PresetDetectionResult(defaultPreset, false);

        }
    }
}
