
// https://github.com/petrroll/PowerSwitcher

using Avalonia.Media;
using System;

namespace PowerSchemaFlyout.Models
{
    public class PowerSchema
    {
        public Guid Guid { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public Color Color { get; init; }

        public PowerSchema(string name, Guid guid, bool isActive)
        {
            this.Name = name;
            this.Guid = guid;
            this.IsActive = isActive;
        }

        public override string ToString()
        {
            return $"{Name}:{IsActive}";
        }

        public static Guid MaximumPerformanceSchemaGuid = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        public static Guid BalancedSchemaGuid = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");
        public static Guid PowerSchemaSaver = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
    }
}
