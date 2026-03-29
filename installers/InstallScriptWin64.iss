; Bombermaaan Installation Script for the C# port
;
; Expected command-line defines:
;   /DAppVersion=3.0.0
;   /DSourceDir=C:\path\to\release\folder
;   /DRepoRoot=C:\path\to\repo

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#ifndef SourceDir
  #define SourceDir "..\releases\win-x64\Bombermaaan_0.0.0"
#endif

#ifndef RepoRoot
  #define RepoRoot ".."
#endif

#define AppName "Bombermaaan"
#define AppPublisher "The Bombermaaan Team"
#define AppURL "https://github.com/bjaraujo/bombermaaan"
#define AppExeName "Bombermaaan.exe"
#define AppId "{{B8B6D8F7-6788-4A58-AF44-8C862F7AF1CB}"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=true
LicenseFile={#SourceDir}\COPYING.txt
OutputDir=output
OutputBaseFilename=Bombermaaan_{#AppVersion}_setup_win64
Compression=lzma
SolidCompression=true
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
VersionInfoVersion={#AppVersion}
VersionInfoDescription=Installer for Bombermaaan
VersionInfoCopyright=Copyright (C) 2000-2002, 2007 Thibaut Tollemer and contributors
SetupIconFile={#RepoRoot}\Bombermaaan\Bombermaaan.ico
UninstallDisplayIcon={app}\{#AppExeName}
WizardStyle=modern

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Parameters: "--use-appdata-dir"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Parameters: "--use-appdata-dir"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Parameters: "--use-appdata-dir"; Flags: nowait postinstall skipifsilent unchecked
Filename: "{app}\README.txt"; Description: "View the README file"; Flags: postinstall shellexec skipifsilent

