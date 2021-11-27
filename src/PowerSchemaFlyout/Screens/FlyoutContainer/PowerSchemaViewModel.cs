// https://github.com/petrroll/PowerSwitcher

using System;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace PowerSchemaFlyout.Screens.FlyoutContainer
{
    public class PowerSchemaViewModel
    {
        public Guid Guid { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public PowerSchemaRol PowerSchemaRol { get; set; }
     
        public PowerSchemaViewModel(string name, Guid guid, bool isActive)
        {
            this.Name = name;
            this.Guid = guid;
        }

        public Symbol Icon
        {
            get
            {
                switch (this.PowerSchemaRol)
                {
                    case PowerSchemaRol.Gaming:
                        return Symbol.Games;
                    case PowerSchemaRol.Desktop:
                        return Symbol.CalendarEmpty;
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