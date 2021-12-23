using System.Linq;
using System.Text;
using PowerSchemaFlyout.Services.Enums;
using PowerSchemaFlyout.Services.Native;

namespace PowerSchemaFlyout.Models.Configuration
{
    public class Preset
    {
        public string Name { get; set; }
        public string ProcessName { get; set; }
        public string? Title { get; set; }
        public ProcessType ProcessType { get; set; }
        public ProcessType InactiveBackProcessType { get; set; }
        public int InactiveTimeout { get; set; }

        public Preset() { }


        public Preset(ProcessWatch processWatch, string name, ProcessType processType, ProcessType inactiveBackProcessType, int inactiveTimeout)
        {
            Name = name;
            ProcessName = processWatch.ProcessName;
            Title = processWatch.Title;
            ProcessType = processType;
            InactiveBackProcessType = inactiveBackProcessType;
            InactiveTimeout = inactiveTimeout;
        }

        public static Preset CreateUnknownPreset()
        {
            return new Preset(ProcessWatch.Empty, string.Empty, ProcessType.Unknown, ProcessType.Unknown, 0);
        }

        public static Preset CreateUnknownPreset(ProcessWatch processWatch)
        {

            return new Preset(processWatch, string.Empty, ProcessType.Unknown, ProcessType.Unknown, 0);
        }

        public static Preset CreateUnknownPreset(ProcessType processType)
        {
            return new Preset(ProcessWatch.Empty, string.Empty, processType, processType, 0);
        }

        public static Preset CreateGamePreset(ProcessWatch processWatch)
        {
            return new Preset(processWatch, processWatch.Title, ProcessType.Game, ProcessType.Game, 0);
        }
      
        public Preset Clone()
        {
            return new Preset()
            {

                Name = this.Name,
                ProcessName = this.ProcessName,
                Title = this.Title,
                ProcessType = this.ProcessType,
                InactiveBackProcessType = this.InactiveBackProcessType,
                InactiveTimeout = this.InactiveTimeout,
            };
        }
    }
}
