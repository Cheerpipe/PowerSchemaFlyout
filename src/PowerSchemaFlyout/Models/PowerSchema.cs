
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
        public Color Color { get; set; }

        public PowerSchema(string name, Guid guid, bool isActive)
        {
            this.Name = name;
            this.Guid = guid;
            this.IsActive = isActive;
        }
    }
}
