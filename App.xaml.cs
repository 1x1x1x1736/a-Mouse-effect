using System.Windows;
using WinForms = System.Windows.Forms;

namespace MouseEffects;

public partial class App : Application
{
    private EffectSettings _settings = null!;
    private OverlayWindow _overlay = null!;
    private ControlPanel? _panel;
    private WinForms.NotifyIcon? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settings = EffectSettings.Load();
        _overlay = new OverlayWindow(_settings);
        _overlay.Show();

        _tray = new WinForms.NotifyIcon
        {
            Text = "Mouse Effects",
            Visible = true,
            Icon = System.Drawing.SystemIcons.Application,
        };

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add("Open Mouse Effects", null, (_, _) => ShowPanel());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());
        _tray.ContextMenuStrip = menu;
        _tray.DoubleClick += (_, _) => ShowPanel();

        ShowPanel();
    }

    private void ShowPanel()
    {
        if (_panel is null)
        {
            _panel = new ControlPanel(_settings);
            _panel.Closed += (_, _) => _panel = null;
            _panel.Show();
        }
        else
        {
            if (_panel.WindowState == WindowState.Minimized)
                _panel.WindowState = WindowState.Normal;

            _panel.Show();
            _panel.Activate();
        }
    }

    private void ExitApp()
    {
        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.Dispose();
            _tray = null;
        }

        Shutdown();
    }
}
