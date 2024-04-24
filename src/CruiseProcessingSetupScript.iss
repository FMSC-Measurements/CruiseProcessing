#define MsBuildOutputDir ".\CruiseProcessing\bin\Release\net472"
#define VERSION "04.23.2024"
#define APP "Cruise Processing Program"
#define BASEURL "https://www.fs.fed.us/forestmanagement/products/measurement"
#define ORGANIZATION "U.S. Forest Service, Forest Management Service Center"

[Setup]
AppName={#APP}
AppMutex=CruiseProcessing
AppID={#APP}
AppVersion={#VERSION}
AppPublisher={#ORGANIZATION}
AppPublisherURL={#BASEURL}/cruising/cruiseprocessing/index.php
AppSupportURL={#BASEURL}/support.shtml
AppUpdatesURL={#BASEURL}/cruising/cruiseprocessing/index.php
; CurPageChanged in the [Code] section checks if C:\fsapps exists. If it does, it uses it as the default install directory.


DefaultDirName={autopf}\FMSC\CruiseProcessing
DefaultGroupName=FMSC\{#APP}

VersionInfoDescription={#APP} Setup
VersionInfoCompany={#ORGANIZATION}
VersionInfoVersion={#VERSION}

Compression=lzma
SolidCompression=yes

PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

AllowNoIcons=yes
LicenseFile=.\FMSC EULA.txt
InfoBeforeFile=.\CruiseProcessingIntro.txt

OutputBaseFilename=CruiseProcessing_{#VERSION}_Setup
OutputManifestFile=Setup-Manifest.txt



[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; 

[Files]
Source: "{#MsBuildOutputDir}\*.exe"; DestDir: {app}; Flags: ignoreversion;
Source: "{#MsBuildOutputDir}\*.dll"; DestDir: {app}; Flags: ignoreversion;
Source: "{#MsBuildOutputDir}\*.exe.config"; DestDir: {app}; Flags: ignoreversion;
Source: "{#MsBuildOutputDir}\runtimes\win-x64\native\*.dll"; DestDir: {app}\runtimes\win-x64\native; Flags: ignoreversion;
Source: "{#MsBuildOutputDir}\runtimes\win-x86\native\*.dll"; DestDir: {app}\runtimes\win-x86\native; Flags: ignoreversion;
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\Cruise Processing Program; Filename: {app}\CruiseProcessing.exe
Name: {autodesktop}\Cruise Processing Program; Filename: {app}\CruiseProcessing.exe; Tasks: desktopicon

[Run]
Filename: "{app}\CruiseProcessing.exe"; Description: "{cm:LaunchProgram,Cruise Processing Program}"; Flags: nowait postinstall skipifsilent
