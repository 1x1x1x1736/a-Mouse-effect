# Mouse Effects

A lightweight Windows desktop app that draws live visual effects over your screen, following the mouse cursor.

- **Cursor Circle** – a pulsing ring that follows the pointer
- **Mouse Trail** – a fading trail behind cursor movement
- **Click Ripple** – an expanding ring on every mouse click

Each effect can be toggled on/off and recolored from a simple control panel. The overlay is fully click-through, so it never gets in the way of your normal work.

## Features

- Transparent, always-on-top overlay (click-through, never intercepts input)
- Three independent effects: cursor circle, trail, click ripple
- Per-effect color pickers and cursor-circle size slider
- Settings are saved automatically to `%AppData%\MouseEffects\settings.json`
- Runs in the system tray — closing the control panel keeps the effects running;
  reopen it from the tray icon (double-click, or right-click → *Open Mouse Effects*)
- Configurable to start automatically with Windows

## Install

1. Download the latest **`MouseEffectsSetup.exe`** from the [Releases](../../releases) page.
2. Run it. It's a **per-user install** — no administrator rights or UAC prompt required.
3. Launch **Mouse Effects** from the Start menu (or from the desktop shortcut, if selected).

The installer is self-contained and bundles everything it needs — you do **not** need to install .NET separately.

To uninstall, use *Settings → Apps → Installed apps → Mouse Effects*, or the uninstaller in the install folder.

## Usage

- The **control panel** opens on launch. Toggle effects, pick colors, adjust the circle size, then click **Save**.
- **Minimize** sends the panel to the tray. **Closing** the panel (X) also keeps the effects running — the app lives in the tray until you exit it from the tray menu.
- To quit completely: right-click the tray icon → **Exit**.

Settings file: `%AppData%\MouseEffects\settings.json`

## Requirements

- Windows 10 or Windows 11 (x64)

## Build from source

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download).

```powershell
# Run from source
dotnet run

# Produce a self-contained, publishable build
dotnet publish MouseEffects.csproj -c Release -r win-x64 --self-contained true -o publish
```

### Build the installer

The installer is built with [Inno Setup 6](https://jrsoftware.org/isinfo.php).

```powershell
dotnet publish MouseEffects.csproj -c Release -r win-x64 --self-contained true -o publish
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" installer.iss
```

The setup program is written to `installer\MouseEffectsSetup.exe`.

> **Note:** do not publish with `PublishSingleFile`. WPF requires its native
> libraries (`wpfgfx_cor3.dll`, `PresentationNative_cor3.dll`, …) to sit next to the
> executable; the installer packs the whole `publish` folder.

## Project layout

| File | Purpose |
| --- | --- |
| `App.xaml` / `App.xaml.cs` | Startup, tray icon, panel lifetime |
| `OverlayWindow.xaml(.cs)` | Click-through full-screen effects overlay |
| `ControlPanel.xaml(.cs)` | Settings window |
| `CursorTracker.cs` | Global cursor position + click tracking |
| `EffectSettings.cs` | Settings model + JSON persistence |
| `installer.iss` | Inno Setup installer script |

## License

No license specified.
