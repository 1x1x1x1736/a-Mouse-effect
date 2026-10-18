; Mouse Effects installer script
;
; Publish first (per architecture):
;   dotnet publish MouseEffects.csproj -c Release -r win-x64   --self-contained true -o publish-x64
;   dotnet publish MouseEffects.csproj -c Release -r win-x86   --self-contained true -o publish-x86
;   dotnet publish MouseEffects.csproj -c Release -r win-arm64 --self-contained true -o publish-arm64
;
; Then compile one installer per architecture (Arch defaults to x64):
;   ISCC.exe installer.iss
;   ISCC.exe /DArch=x86   installer.iss
;   ISCC.exe /DArch=arm64 installer.iss
;
; Note: do NOT use PublishSingleFile - WPF needs its native libraries
; (wpfgfx_cor3.dll, PresentationNative_cor3.dll, ...) next to the exe.

#define MyAppName "Mouse Effects"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Mouse Effects"
#define MyAppExeName "MouseEffects.exe"

; Architecture: "x64" (default), "x86" or "arm64"
#ifndef Arch
  #define Arch "x64"
#endif

#if Arch == "x86"
  #define PublishDir "publish-x86"
#elif Arch == "arm64"
  #define PublishDir "publish-arm64"
#else
  #define PublishDir "publish-x64"
#endif

[Setup]
AppId={{B7E4A1D2-6C8F-4A3E-9F5B-2D8C7A1E4F60}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=MouseEffects_win_{#Arch}_{#MyAppVersion}_setup
Compression=lzma2/max
SolidCompression=yes
#if Arch == "x86"
ArchitecturesAllowed=x86compatible
#elif Arch == "arm64"
ArchitecturesAllowed=arm64
ArchitecturesInstallIn64BitMode=arm64
#else
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
#endif
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
Source: "{#PublishDir}\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
