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

    private readonly EffectSettings _settings;
    private readonly DispatcherTimer _saveTimer;
    private bool _isInitializing = true;

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

        BuildSwatches(CircleSwatches, "Circle", settings.CircleColor);
        BuildSwatches(TrailSwatches, "Trail", settings.TrailColor);
        BuildSwatches(RippleSwatches, "Ripple", settings.RippleColor);

        CircleToggle.IsChecked = settings.CircleEnabled;
        TrailToggle.IsChecked = settings.TrailEnabled;
        RippleToggle.IsChecked = settings.RippleEnabled;
        SizeSlider.Value = settings.CircleSize;
        _isInitializing = false;
    }

    private void BuildSwatches(WrapPanel panel, string groupName, Color selected)
    {
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

    private void Swatch_Checked(object sender, RoutedEventArgs e)
    {
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

        ScheduleSave();
    }

    private void CircleToggle_Changed(object sender, RoutedEventArgs e)
    {
        _settings.CircleEnabled = CircleToggle.IsChecked == true;
        ScheduleSave();
    }

    private void TrailToggle_Changed(object sender, RoutedEventArgs e)
    {
        _settings.TrailEnabled = TrailToggle.IsChecked == true;
        ScheduleSave();
    }

    private void RippleToggle_Changed(object sender, RoutedEventArgs e)
    {
        _settings.RippleEnabled = RippleToggle.IsChecked == true;
        ScheduleSave();
    }

    private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _settings.CircleSize = e.NewValue;
        if (SizeLabel != null)
            SizeLabel.Text = ((int)e.NewValue).ToString();
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
