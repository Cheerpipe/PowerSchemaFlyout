// https://github.com/petrroll/PowerSwitcher

using System;
using Avalonia.Media;
using Material.Icons;

namespace PowerSchemaFlyout.Screens.FlyoutContainer
{
    public class PowerSchemaViewModel
    {
        public Guid Guid { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public PowerSchemaRol PowerSchemaRol { get; set; }

        public PowerSchemaViewModel(string name, Guid guid)
        {
            this.Name = name;
            this.Guid = guid;
        }

        public PowerSchemaViewModel(string name, Guid guid, bool isActive)
        {
            this.Name = name;
            this.Guid = guid;
            this.IsActive = isActive;
        }

        public MaterialIconKind Icon
        {
            get
            {
                switch (this.PowerSchemaRol)
                {
                    case PowerSchemaRol.Gaming:
                        return MaterialIconKind.Speedometer;
                    case PowerSchemaRol.Desktop:
                        return MaterialIconKind.SpeedometerMedium;
                    case PowerSchemaRol.PowerSaving:
                        return MaterialIconKind.SpeedometerSlow;
                    default:
                        return 0;
                }
            }
        }

        public SolidColorBrush Foreground
        {
            get
            {
                switch (this.PowerSchemaRol)
                {
                    case PowerSchemaRol.Gaming:
                        return new SolidColorBrush(Colors.White);
                    case PowerSchemaRol.Desktop:
                        return new SolidColorBrush(Colors.White);
                    case PowerSchemaRol.PowerSaving:
                        return new SolidColorBrush(Colors.White);
                    default:
                        return new SolidColorBrush(Colors.Transparent);
                }
            }
        }
    }

    public enum PowerSchemaRol
    {
        Unknown,
        PowerSaving,
        Desktop,
        Gaming,
    }
}