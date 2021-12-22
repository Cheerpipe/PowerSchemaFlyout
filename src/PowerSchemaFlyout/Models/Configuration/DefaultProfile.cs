using PowerSchemaFlyout.Services.Enums;

namespace PowerSchemaFlyout.Models.Configuration
{
    public class DefaultProfile
    {
        public ProcessType ProcessType { get; set; }
        public ProcessType InactiveBackProcessType { get; set; }
        public int InactiveTimeout { get; set; }
    }
}
