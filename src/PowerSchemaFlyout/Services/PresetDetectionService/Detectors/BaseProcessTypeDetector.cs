using PowerSchemaFlyout.IoC;
using PowerSchemaFlyout.Services.Configuration;
using PowerSchemaFlyout.Services.Native;
using Serilog;

namespace PowerSchemaFlyout.Services.Detectors
{
    public abstract class BaseProcessTypeDetector: IProcessTypeDetector
    {
        protected readonly IConfigurationService ConfigurationService;
        protected readonly ILogger Logger;
        public BaseProcessTypeDetector()
        {
            ConfigurationService = Kernel.Get<IConfigurationService>();
            Logger = Kernel.Get<ILogger>();
        }

        public virtual PresetDetectionResult DetectProcessType(ProcessWatch process, PresetDetectionResult currentResult)
        {
            throw new System.NotImplementedException();
        }
    }
}
