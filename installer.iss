; Mouse Effects installer script
; Publish first:
;   dotnet publish MouseEffects.csproj -c Release -r win-x64 --self-contained true -o publish
; Then compile:
;   ISCC.exe installer.iss
;
; Note: do NOT use PublishSingleFile - WPF needs its native libraries
; (wpfgfx_cor3.dll, PresentationNative_cor3.dll, ...) next to the exe.

#define MyAppName "Mouse Effects"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Mouse Effects"
#define MyAppExeName "MouseEffects.exe"

[Setup]
AppId={{B7E4A1D2-6C8F-4A3E-9F5B-2D8C7A1E4F60}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=MouseEffectsSetup
Compression=lzma2/max
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"
Name: "startup"; Description: "Start {#MyAppName} automatically when Windows starts"; GroupDescription: "Startup:"

[Files]
Source: "publish\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
