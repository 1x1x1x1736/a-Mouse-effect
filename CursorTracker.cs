using System.Runtime.InteropServices;
using System.Windows;

namespace MouseEffects;

public sealed class CursorTracker
{
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VK_LBUTTON = 0x01;

    private readonly List<(Point Pos, double Ticks)> _trail = new();
    private const int MaxTrailPoints = 24;
    private bool _wasDown;

    public Point CursorPos { get; private set; }
    public IReadOnlyList<(Point Pos, double Ticks)> Trail => _trail;
    public List<Point> NewClicks { get; } = new();

    public void Update(double nowTicks)
    {
        GetCursorPos(out var p);
        CursorPos = new Point(p.X, p.Y);

        _trail.Add((CursorPos, nowTicks));
        while (_trail.Count > MaxTrailPoints)
            _trail.RemoveAt(0);

        bool down = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
        if (down && !_wasDown)
            NewClicks.Add(CursorPos);
        _wasDown = down;
    }
}
