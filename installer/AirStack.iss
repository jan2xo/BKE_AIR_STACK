#ifndef AppVersion
  #error AppVersion must be supplied by the packaging script.
#endif

#ifndef SourceDir
  #error SourceDir must be supplied by the packaging script.
#endif

#ifndef OutputDir
  #error OutputDir must be supplied by the packaging script.
#endif

[Setup]
AppId={{3052E18F-2EF4-4771-B60B-86294FBC70DC}
AppName=Air Stack
AppVersion={#AppVersion}
AppVerName=Air Stack {#AppVersion}
AppPublisher=BKE Digital Solutions
DefaultDirName={autopf}\BKE AirStack
DefaultGroupName=Air Stack
DisableProgramGroupPage=yes
UninstallDisplayName=Air Stack
UninstallDisplayIcon={app}\BKE AirStack.exe
OutputDir={#OutputDir}
OutputBaseFilename=Air-Stack-{#AppVersion}-Windows-x64
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
SetupLogging=yes
VersionInfoVersion={#AppVersion}.0
VersionInfoCompany=BKE Digital Solutions
VersionInfoDescription=Air Stack Windows x64 installer
VersionInfoProductName=Air Stack
VersionInfoProductVersion={#AppVersion}

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Air Stack"; Filename: "{app}\BKE AirStack.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\BKE AirStack.exe"; Description: "Launch Air Stack"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent
