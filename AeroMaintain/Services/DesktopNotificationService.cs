using AeroMaintain.Models;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace AeroMaintain.Services;

public sealed class DesktopNotificationService : IDisposable
{
    private Forms.NotifyIcon? _notifyIcon;

    public void ShowOperationalAlertSummary(IEnumerable<MaintenanceAlert> alerts, string summary)
    {
        var alertList = alerts.ToList();
        if (alertList.Count == 0)
        {
            return;
        }

        EnsureNotifyIcon();
        _notifyIcon!.BalloonTipTitle = "AeroMaintain operational alerts";
        _notifyIcon.BalloonTipText = BuildBalloonText(alertList, summary);
        _notifyIcon.BalloonTipIcon = alertList.Any(alert => alert.IsCritical)
            ? Forms.ToolTipIcon.Warning
            : Forms.ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(8000);
    }

    public void Dispose()
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _notifyIcon = null;
    }

    private void EnsureNotifyIcon()
    {
        if (_notifyIcon is not null)
        {
            return;
        }

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Warning,
            Text = "AeroMaintain alerts",
            Visible = true
        };
    }

    private static string BuildBalloonText(IReadOnlyCollection<MaintenanceAlert> alerts, string summary)
    {
        var topAlerts = alerts
            .Take(3)
            .Select(alert => alert.Message)
            .ToList();

        var text = topAlerts.Count == 0
            ? summary
            : string.Join(Environment.NewLine, topAlerts);

        return text.Length <= 255 ? text : text[..252] + "...";
    }
}
