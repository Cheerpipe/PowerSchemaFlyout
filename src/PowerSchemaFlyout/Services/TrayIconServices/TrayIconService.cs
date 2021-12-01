using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaTrayIcon = Avalonia.Controls.TrayIcon;

namespace PowerSchemaFlyout.Services
{
    public class TrayIconService : ITrayIconService, IDisposable
    {
        private readonly IFlyoutService _flyoutService;
        private readonly AvaloniaTrayIcon _trayIcon;

        public TrayIconService(IFlyoutService flyoutService)
        {
            _flyoutService = flyoutService;
            _flyoutService.PreLoad();
            _trayIcon = new AvaloniaTrayIcon();
            _trayIcon.Clicked += TrayIcon_Clicked;
        }

        public void Show()
        {
            _trayIcon.Menu = new NativeMenu();
            NativeMenuItem exitMenu = new("Exit Power Schema Flyout");
            exitMenu.Click += ExitMenu_Click;
            _trayIcon.Menu.Items.Add(exitMenu);
            _trayIcon.IsVisible = true;
        }
        public void Hide()
        {
            _trayIcon.IsVisible = false;
        }

        public void UpdateIcon(string iconName)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var icon = new WindowIcon(assets.Open(new Uri(@$"resm:PowerSchemaFlyout.Assets.{iconName}")));
            Dispatcher.UIThread.Post(() => { _trayIcon.Icon = icon; });
        }

        public void UpdateTooltip(string tooltipText)
        {
            Dispatcher.UIThread.Post(() => { _trayIcon.ToolTipText = tooltipText; });
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Program.RunCancellationTokenSource.Cancel();
        }

        private void TrayIcon_Clicked(object sender, EventArgs e)
        {
            _flyoutService.Toggle();
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
        }
    }
}
