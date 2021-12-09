
namespace PowerSchemaFlyout.Services
{
    public interface ITrayIconService
    {
        void Show();
        void Hide();
        void UpdateIcon(string iconName);
        void UpdateTooltip(string tooltipText);
        void Refresh();
    }
}
