using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MouseEffects;

public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_EX_LAYERED = 0x80000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const double TrailLifeMs = 380;
    private const double RippleDurationMs = 500;
    private const double MaxTrailJump = 300;

    private sealed class Ripple
    {
        public Point Pos;
        public double StartMs;
    }

    private readonly EffectSettings _settings;
    private readonly CursorTracker _tracker = new();
    private readonly DispatcherTimer _timer;
    private readonly List<Ripple> _ripples = new();
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    private SolidColorBrush? _circleBrush;
    private Color _circleBrushColor;
    private SolidColorBrush? _trailBrush;
    private Color _trailBrushColor;
    private SolidColorBrush? _rippleBrush;
    private Color _rippleBrushColor;

    public OverlayWindow(EffectSettings settings)
    {
        _settings = settings;
        InitializeComponent();

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        Loaded += OnLoaded;
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16),
        };
        _timer.Tick += (_, _) => Frame();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        int ex = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, ex | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW);
        _timer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        base.OnClosed(e);
    }

    private void Frame()
    {
        double now = _sw.ElapsedMilliseconds;

        _tracker.NewClicks.Clear();
        _tracker.Update(now);

        foreach (Point c in _tracker.NewClicks)
            _ripples.Add(new Ripple { Pos = ToLocal(c), StartMs = now });

        _ripples.RemoveAll(r => now - r.StartMs > RippleDurationMs);

        FxCanvas.Children.Clear();

        if (_settings.TrailEnabled)
            DrawTrail(now);

        if (_settings.RippleEnabled)
            DrawRipples(now);

        if (_settings.CircleEnabled)
            DrawCircle(now);
    }

    private void DrawTrail(double now)
    {
        IReadOnlyList<(Point Pos, double Ticks)> trail = _tracker.Trail;
        if (trail.Count < 2)
            return;

        SolidColorBrush brush = Cached(ref _trailBrush, ref _trailBrushColor, _settings.TrailColor);

        for (int i = 1; i < trail.Count; i++)
        {
            double age = now - trail[i].Ticks;
            if (age > TrailLifeMs)
                continue;

            Point a = ToLocal(trail[i - 1].Pos);
            Point b = ToLocal(trail[i].Pos);
            if ((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) > MaxTrailJump * MaxTrailJump)
                continue;

            double k = 1 - age / TrailLifeMs;
            FxCanvas.Children.Add(new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = brush,
                StrokeThickness = 2 + 5 * k,
                Opacity = k * k,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            });
        }
    }

    private void DrawRipples(double now)
    {
        SolidColorBrush brush = Cached(ref _rippleBrush, ref _rippleBrushColor, _settings.RippleColor);

        foreach (Ripple r in _ripples)
        {
            double t = (now - r.StartMs) / RippleDurationMs;
            double radius = 8 + 48 * (1 - (1 - t) * (1 - t));
            var e = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = brush,
                StrokeThickness = 3 * (1 - t),
                Opacity = 1 - t,
            };
            Canvas.SetLeft(e, r.Pos.X - radius);
            Canvas.SetTop(e, r.Pos.Y - radius);
            FxCanvas.Children.Add(e);
        }
    }

    private void DrawCircle(double now)
    {
        Point pos = ToLocal(_tracker.CursorPos);
        double pulse = 1 + 0.15 * Math.Sin(now / 180.0);
        double radius = _settings.CircleSize / 2 * pulse;
        SolidColorBrush brush = Cached(ref _circleBrush, ref _circleBrushColor, _settings.CircleColor);

        var ring = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = brush,
            StrokeThickness = 3,
            Opacity = 0.9,
        };
        Canvas.SetLeft(ring, pos.X - radius);
        Canvas.SetTop(ring, pos.Y - radius);
        FxCanvas.Children.Add(ring);

        var dot = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = brush,
            Opacity = 0.9,
        };
        Canvas.SetLeft(dot, pos.X - 3);
        Canvas.SetTop(dot, pos.Y - 3);
        FxCanvas.Children.Add(dot);
    }

    private Point ToLocal(Point screenPhys)
    {
        if (PresentationSource.FromVisual(this)?.CompositionTarget is { } target)
        {
            Point dip = target.TransformFromDevice.Transform(screenPhys);
            return new Point(
                dip.X - SystemParameters.VirtualScreenLeft,
                dip.Y - SystemParameters.VirtualScreenTop);
        }

        return screenPhys;
    }

    private static SolidColorBrush Cached(ref SolidColorBrush? brush, ref Color cache, Color color)
    {
        if (brush is null || cache != color)
        {
            brush = new SolidColorBrush(color);
            cache = color;
        }

        return brush;
    }
}
