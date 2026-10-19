using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MouseEffects;

public sealed class ThemePreset
{
    public string Name { get; set; } = string.Empty;
    public string? CircleHex { get; set; }
    public string? TrailHex { get; set; }
    public string? RippleHex { get; set; }
    public bool CircleEnabled { get; set; } = true;
    public bool TrailEnabled { get; set; } = true;
    public bool RippleEnabled { get; set; } = true;
}

public sealed class EffectSettings
{
    private static string SavePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MouseEffects",
            "settings.json");

    public bool CircleEnabled { get; set; } = true;
    public bool TrailEnabled { get; set; } = true;
    public bool RippleEnabled { get; set; } = true;

    public Color CircleColor { get; set; } = Color.FromRgb(0xFF, 0x45, 0x00);
    public Color TrailColor { get; set; } = Color.FromRgb(0x00, 0xBC, 0xD4);
    public Color RippleColor { get; set; } = Colors.White;

    public double CircleSize { get; set; } = 40;
    public string? SelectedTheme { get; set; }

    public void ApplyPreset(ThemePreset preset)
    {
        if (!string.IsNullOrWhiteSpace(preset.CircleHex))
            CircleColor = FromHex(preset.CircleHex, CircleColor);

        if (!string.IsNullOrWhiteSpace(preset.TrailHex))
            TrailColor = FromHex(preset.TrailHex, TrailColor);

        if (!string.IsNullOrWhiteSpace(preset.RippleHex))
            RippleColor = FromHex(preset.RippleHex, RippleColor);

        CircleEnabled = preset.CircleEnabled;
        TrailEnabled = preset.TrailEnabled;
        RippleEnabled = preset.RippleEnabled;
        SelectedTheme = preset.Name;
    }

    public void Save()
    {
        var dto = new Dto
        {
            CircleEnabled = CircleEnabled,
            TrailEnabled = TrailEnabled,
            RippleEnabled = RippleEnabled,
            CircleColor = ToHex(CircleColor),
            TrailColor = ToHex(TrailColor),
            RippleColor = ToHex(RippleColor),
            CircleSize = CircleSize,
            SelectedTheme = SelectedTheme,
        };

        Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
        File.WriteAllText(SavePath, JsonSerializer.Serialize(dto, JsonOptions));
    }

    public static EffectSettings Load()
    {
        if (!File.Exists(SavePath))
            return new EffectSettings();

        try
        {
            var dto = JsonSerializer.Deserialize<Dto>(File.ReadAllText(SavePath), JsonOptions);
            if (dto is null)
                return new EffectSettings();

            return new EffectSettings
            {
                CircleEnabled = dto.CircleEnabled,
                TrailEnabled = dto.TrailEnabled,
                RippleEnabled = dto.RippleEnabled,
                CircleColor = FromHex(dto.CircleColor, Colors.OrangeRed),
                TrailColor = FromHex(dto.TrailColor, Colors.Cyan),
                RippleColor = FromHex(dto.RippleColor, Colors.White),
                CircleSize = dto.CircleSize,
                SelectedTheme = dto.SelectedTheme,
            };
        }
        catch
        {
            return new EffectSettings();
        }
    }

    private static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static Color FromHex(string? hex, Color fallback) =>
        string.IsNullOrWhiteSpace(hex) ? fallback : (Color)ColorConverter.ConvertFromString(hex)!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    private sealed class Dto
    {
        public bool CircleEnabled { get; set; } = true;
        public bool TrailEnabled { get; set; } = true;
        public bool RippleEnabled { get; set; } = true;
        public string CircleColor { get; set; } = "#FF4500";
        public string TrailColor { get; set; } = "#00BCD4";
        public string RippleColor { get; set; } = "#FFFFFF";
        public double CircleSize { get; set; } = 40;
        public string? SelectedTheme { get; set; }
    }
}
