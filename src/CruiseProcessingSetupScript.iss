; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!


[Setup]
AppName=Cruise Processing Program
AppVerName=Cruise Processing 06.01.2022
AppPublisher=Forest Management Service Center
AppPublisherURL=http://www.fs.fed.us/fmsc/measure/cruising/cruiseprocessing/index.php
AppSupportURL=http://www.fs.fed.us/fmsc/measure/support.shtml
AppUpdatesURL=http://www.fs.fed.us/fmsc/measure/cruising/cruiseprocessing/index.php
AppMutex=CruiseProcessing
; CurPageChanged in the [Code] section checks if C:\fsapps exists. If it does, it uses it as the default install directory.
DefaultDirName={autopf}\FMSC\CruiseProcessing

DefaultGroupName=FMSC\Cruise Processing Program
AllowNoIcons=yes
LicenseFile=C:\WorkSpace\CruiseProcessingReleases\InstallScript\FMSC EULA.txt
InfoBeforeFile=C:\WorkSpace\CruiseProcessingReleases\InstallScript\CruiseProcessingIntro.txt
OutputDir=C:\WorkSpace\CruiseProcessingReleases\ReleaseInstallers
OutputBaseFilename=CruiseProcessing20220601_Setup
OutputManifestFile=Setup-Manifest.txt
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; 

[Files]
Source: C:\WorkSpace\CruiseProcessingReleases\InstallScript\*; DestDir: {app}; Flags: ignoreversion recursesubdirs
;Source: C:\WorkSpace\CruiseProcessingReleases\InstallScript\*.dll; DestDir: {app}; Flags: ignoreversion
;Source: C:\WorkSpace\CruiseProcessingReleases\InstallScript\runtimes\*; DestDir: {app}; Flags: ignoreversion recursesubdirs
; Source: C:\WorkSpace\CruiseProcessingReleases\InstallScript\*.db; DestDir: {app}; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\Cruise Processing Program; Filename: {app}\CruiseProcessing.exe
Name: {autodesktop}\Cruise Processing Program; Filename: {app}\CruiseProcessing.exe; Tasks: desktopicon

[Run]
Filename: "{app}\CruiseProcessing.exe"; Description: "{cm:LaunchProgram,Cruise Processing Program}"; Flags: nowait postinstall skipifsilent
