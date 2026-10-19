using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MouseEffects;

public partial class ControlPanel : Window
{
    private static readonly (string Name, string Hex)[] Palette =
    [
        ("Orange Red", "#FF4500"),
        ("Red", "#F44336"),
        ("Orange", "#FF9800"),
        ("Yellow", "#FFEB3B"),
        ("Green", "#4CAF50"),
        ("Cyan", "#00BCD4"),
        ("Blue", "#2196F3"),
        ("Purple", "#9C27B0"),
        ("White", "#FFFFFF"),
    ];

    private static readonly ThemePreset[] Presets =
    [
        new() { Name = "Default", CircleHex = "#FF4500", TrailHex = "#00BCD4", RippleHex = "#FFFFFF", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Neon Blue", CircleHex = "#4D7CFE", TrailHex = "#60A5FA", RippleHex = "#93C5FD", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Neon Purple", CircleHex = "#A78BFA", TrailHex = "#C084FC", RippleHex = "#E9D5FF", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Neon Pink", CircleHex = "#FB7185", TrailHex = "#F472B6", RippleHex = "#F9A8D4", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Neon Green", CircleHex = "#34D399", TrailHex = "#10B981", RippleHex = "#6EE7B7", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Cyber Cyan", CircleHex = "#22D3EE", TrailHex = "#06B6D4", RippleHex = "#67E8F9", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Fire", CircleHex = "#FF3C00", TrailHex = "#FF6B00", RippleHex = "#FF9F00", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Ice", CircleHex = "#BFDBFE", TrailHex = "#E0F2FE", RippleHex = "#F8FAFC", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Matrix", CircleHex = "#22C55E", TrailHex = "#4ADE80", RippleHex = "#86EFAC", CircleEnabled = true, TrailEnabled = true, RippleEnabled = true },
        new() { Name = "Minimal", CircleHex = "#FFFFFF", TrailHex = "#FFFFFF", RippleHex = "#FFFFFF", CircleEnabled = true, TrailEnabled = false, RippleEnabled = true },
        new() { Name = "Ghost", CircleHex = "#E5E7EB", TrailHex = "#D1D5DB", RippleHex = "#F3F4F6", CircleEnabled = true, TrailEnabled = true, RippleEnabled = false },
        new() { Name = "Clean", CircleHex = "#9CA3AF", TrailHex = "#6B7280", RippleHex = "#D1D5DB", CircleEnabled = true, TrailEnabled = false, RippleEnabled = false },
    ];

    private readonly EffectSettings _settings;
    private readonly DispatcherTimer _saveTimer;
    private bool _isInitializing = true;
    private bool _updatingFromPreset = false;

    public ControlPanel(EffectSettings settings)
    {
        _settings = settings;
        InitializeComponent();

        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(400),
        };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            SaveSettings();
        };
        Closing += ControlPanel_Closing;

        foreach (var p in Presets)
            ThemeCombo.Items.Add(p.Name);

        BuildSwatches(CircleSwatches, "Circle", settings.CircleColor);
        BuildSwatches(TrailSwatches, "Trail", settings.TrailColor);
        BuildSwatches(RippleSwatches, "Ripple", settings.RippleColor);

        CircleToggle.IsChecked = settings.CircleEnabled;
        TrailToggle.IsChecked = settings.TrailEnabled;
        RippleToggle.IsChecked = settings.RippleEnabled;
        SizeSlider.Value = settings.CircleSize;

        if (!string.IsNullOrWhiteSpace(settings.SelectedTheme))
            ThemeCombo.SelectedItem = settings.SelectedTheme;
        else
            ThemeCombo.SelectedItem = "Default";

        _isInitializing = false;
    }

    private void BuildSwatches(WrapPanel panel, string groupName, Color selected)
    {
        panel.Children.Clear();
        foreach ((string name, string hex) in Palette)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex)!;
            var rb = new RadioButton
            {
                GroupName = groupName,
                Tag = hex,
                ToolTip = name,
                Background = new SolidColorBrush(color),
                IsChecked = color == selected,
                Style = (Style)FindResource("Swatch"),
            };
            rb.Checked += Swatch_Checked;
            panel.Children.Add(rb);
        }
    }

    private void SyncSwatches()
    {
        BuildSwatches(CircleSwatches, "Circle", _settings.CircleColor);
        BuildSwatches(TrailSwatches, "Trail", _settings.TrailColor);
        BuildSwatches(RippleSwatches, "Ripple", _settings.RippleColor);
    }

    private void Swatch_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingFromPreset)
            return;

        if (sender is not RadioButton { Tag: string hex } rb)
            return;

        var color = (Color)ColorConverter.ConvertFromString(hex)!;
        switch (rb.GroupName)
        {
            case "Circle":
                _settings.CircleColor = color;
                break;
            case "Trail":
                _settings.TrailColor = color;
                break;
            case "Ripple":
                _settings.RippleColor = color;
                break;
        }

        _settings.SelectedTheme = null;
        if (ThemeCombo.SelectedIndex >= 0)
            ThemeCombo.SelectedItem = null;

        ScheduleSave();
    }

    private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        if (ThemeCombo.SelectedItem is not string name)
            return;

        var preset = Presets.FirstOrDefault(p => p.Name == name);
        if (preset is null)
            return;

        _updatingFromPreset = true;
        try
        {
            _settings.ApplyPreset(preset);
            CircleToggle.IsChecked = _settings.CircleEnabled;
            TrailToggle.IsChecked = _settings.TrailEnabled;
            RippleToggle.IsChecked = _settings.RippleEnabled;
            SizeSlider.Value = _settings.CircleSize;
            SyncSwatches();
            ScheduleSave();
        }
        finally
        {
            _updatingFromPreset = false;
        }
    }

    private void CircleToggle_Changed(object sender, RoutedEventArgs e)
    {
        _settings.CircleEnabled = CircleToggle.IsChecked == true;
        _settings.SelectedTheme = null;
        if (ThemeCombo.SelectedIndex >= 0)
            ThemeCombo.SelectedItem = null;
        ScheduleSave();
    }

    private void TrailToggle_Changed(object sender, RoutedEventArgs e)
    {
        _settings.TrailEnabled = TrailToggle.IsChecked == true;
        _settings.SelectedTheme = null;
        if (ThemeCombo.SelectedIndex >= 0)
            ThemeCombo.SelectedItem = null;
        ScheduleSave();
    }

    private void RippleToggle_Changed(object sender, RoutedEventArgs e)
    {
        _settings.RippleEnabled = RippleToggle.IsChecked == true;
        _settings.SelectedTheme = null;
        if (ThemeCombo.SelectedIndex >= 0)
            ThemeCombo.SelectedItem = null;
        ScheduleSave();
    }

    private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _settings.CircleSize = e.NewValue;
        if (SizeLabel != null)
            SizeLabel.Text = ((int)e.NewValue).ToString();
        _settings.SelectedTheme = null;
        if (ThemeCombo.SelectedIndex >= 0)
            ThemeCombo.SelectedItem = null;
        ScheduleSave();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        _saveTimer.Stop();
        if (!SaveSettings())
            return;

        SaveStatus.Visibility = Visibility.Visible;
        await Task.Delay(2000);
        SaveStatus.Visibility = Visibility.Collapsed;
    }

    private void Exit_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void ControlPanel_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _saveTimer.Stop();
        SaveSettings();
    }

    private void ScheduleSave()
    {
        if (_isInitializing)
            return;

        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private bool SaveSettings()
    {
        try
        {
            _settings.Save();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save settings:\n{ex.Message}", "Mouse Effects",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }
}
