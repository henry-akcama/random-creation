; ============================================================================
; Random Creation — Inno Setup installer script
; ============================================================================
; Decisions this script implements (RandomCreation_DevelopmentLifecycle.md §6):
;   * PER-USER install: lands in the user's own folder, needs no administrator
;     rights, raises no UAC prompt — which matters more than usual because the
;     app is unsigned.
;   * Install location is CHOOSABLE. The default is per-user; anyone who
;     redirects it to Program Files will then need admin — expected, not a bug.
;   * portable.txt is EXCLUDED — its absence is what tells the app to keep
;     user data in %LocalAppData%\RandomCreation instead of beside the exe.
;   * User data is NEVER touched by install or upgrade. The uninstaller asks
;     whether to also remove it and DEFAULTS TO KEEPING it — the safe answer
;     must be the one you get by clicking through without reading.
;   * The version comes from the build (the git tag, via /DAppVersion) so a
;     forgotten hand-edit can never ship a wrongly-labelled installer.
;
; Build (GitHub Actions, or locally with Inno Setup 6 installed):
;   iscc /DAppVersion=4.0.0 /DSourceDir=<publish folder> RandomCreation.iss
; ============================================================================

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\RandomCreation\bin\Release\net8.0-windows\win-x64\publish"
#endif

[Setup]
; AppId identifies this app to Windows across releases — never change it,
; or upgrades will install side by side instead of replacing.
AppId={{7C31A9E4-52D8-4B6F-9A0D-3E8F41C6B7D2}
AppName=Random Creation
AppVersion={#AppVersion}
AppPublisher=akcama
AppPublisherURL=https://github.com/akcama/random-creation
AppSupportURL=https://github.com/akcama/random-creation
DefaultDirName={localappdata}\Programs\Random Creation
DisableDirPage=no
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputBaseFilename=RandomCreation-{#AppVersion}-setup
SetupIconFile=..\RandomCreation\icon.ico
UninstallDisplayIcon={app}\RandomCreation.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; Flags: unchecked

[Files]
; Everything dotnet publish produced, except the portable marker.
Source: "{#SourceDir}\*"; DestDir: "{app}"; \
  Excludes: "portable.txt"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{userprograms}\Random Creation"; Filename: "{app}\RandomCreation.exe"
Name: "{userdesktop}\Random Creation"; Filename: "{app}\RandomCreation.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\RandomCreation.exe"; Description: "Launch Random Creation"; \
  Flags: nowait postinstall skipifsilent

[Code]
// The uninstaller asks whether to also remove user content, defaulting to
// KEEP (MB_DEFBUTTON2 puts the default on No). Someone uninstalling in order
// to reinstall must not lose their collections through inattention.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataDir: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DataDir := ExpandConstant('{localappdata}\RandomCreation');
    if DirExists(DataDir) then
    begin
      if MsgBox('Do you also want to remove your collections, history, presets'
                + #13#10 + 'and settings?' + #13#10 + #13#10
                + 'Choosing No keeps them, and a future reinstall will pick'
                + #13#10 + 'them up exactly as they were.',
                mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
        DelTree(DataDir, True, True, True);
    end;
  end;
end;
