#define MyAppName "Air Stack"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "BKE Digital Solutions"
#define MyAppExeName "BKE AirStack.exe"

[Setup]
AppId={{8E8DB594-27D6-4C60-87DA-16E1C0A63B43}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\BKE\Air Stack
DefaultGroupName=BKE\Air Stack
DisableProgramGroupPage=yes
OutputDir=..\installer-output
OutputBaseFilename=Air-Stack-{#MyAppVersion}-Windows-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayName={#MyAppName}
SetupIconFile=..\BKE AirStack\BKE LOGO.ico

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\BKE\Air Stack"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Air Stack"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Air Stack"; Flags: nowait postinstall skipifsilent
