// https://github.com/petrroll/PowerSwitcher

using System;
using Avalonia.Media;

namespace PowerSchemaFlyout.Screens
{
    public class PowerSchemaViewModel
    {
        public Guid Guid { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public Color Color { get; set; }

        public SolidColorBrush Brush
        {
            get
            {
                return new SolidColorBrush(Color);
            }
        }

        public PowerSchemaViewModel(string name, Guid guid, bool isActive, Color color)
        {
            this.Name = name;
            this.Guid = guid;
            this.IsActive = isActive;
            this.Color = color;
        }
    }
}