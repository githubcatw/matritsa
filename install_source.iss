; Сборка полного установщика Матрицы.

[Setup]
AppName=Матрица
AppVersion=1.0-beta5
WizardStyle=modern
DefaultDirName={autopf}\Matritsa
DefaultGroupName=Матрица
UninstallDisplayIcon={app}\matrigen.exe
Compression=lzma2
SolidCompression=yes
OutputDir=userdocs:Inno Setup Output
OutputBaseFilename=setup-matritsa

[Files]
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\matrigen.exe"; DestDir: "{app}"
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\*.dll"; DestDir: "{app}"
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\*.json"; DestDir: "{app}"
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win\*"; DestDir: "{app}\runtimes\win"; Flags: recursesubdirs
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win-arm64\*"; DestDir: "{app}\runtimes\win-arm64"; Flags: recursesubdirs
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win-x64\*"; DestDir: "{app}\runtimes\win-x64"; Flags: recursesubdirs
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win-x86\*"; DestDir: "{app}\runtimes\win-x86"; Flags: recursesubdirs
; add .net 6
Source: "dependencies\arm64\dotnet-windows-rt-6.0.36.exe"; Flags: deleteafterinstall; DestDir: {tmp}; AfterInstall: InstallFramework; Check: FrameworkIsNotInstalledA64
Source: "dependencies\x64\dotnet-windows-rt-6.0.36.exe"; Flags: deleteafterinstall; DestDir: {tmp}; AfterInstall: InstallFramework; Check: FrameworkIsNotInstalledX64
Source: "dependencies\x86\dotnet-windows-rt-6.0.36.exe"; Flags: deleteafterinstall; DestDir: {tmp}; AfterInstall: InstallFramework; Check: FrameworkIsNotInstalledX86
; add vcredist (mostly for win7)
Source: "dependencies\x64\vcredist.exe"; DestDir: {tmp}; Flags: deleteafterinstall; Check: IsX64OS; AfterInstall: InstallVCRedist
Source: "dependencies\x86\vcredist.exe"; DestDir: {tmp}; Flags: deleteafterinstall; Check: IsX86OS; AfterInstall: InstallVCRedist
; add legacy launcher
Source: "leglaunch\leglaunch\bin\Release\leglaunch.exe"; DestDir: "{app}"; Check: IsX86OS

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Code]
function FrameworkIsNotInstalledA64: Boolean;
begin
  Result := Is64BitInstallMode and (ProcessorArchitecture = paARM64) and not FileOrDirExists(ExpandConstant('{autopf}\dotnet\shared\Microsoft.WindowsDesktop.App\6.0.36'));
end;
function FrameworkIsNotInstalledX64: Boolean;
begin
  Result := IsX64OS and not FileOrDirExists(ExpandConstant('{autopf}\dotnet\shared\Microsoft.WindowsDesktop.App\6.0.36'));
end;
function FrameworkIsNotInstalledX86: Boolean;
begin
  Result := IsX86OS and not FileOrDirExists(ExpandConstant('{autopf}\dotnet\shared\Microsoft.WindowsDesktop.App\6.0.36'));
end;

function LinkToMatrigen: Boolean;
begin
  Result := IsX64OS or IsArm64
end;

procedure InstallFramework;
var
  ResultCode: Integer;
begin
  WizardForm.StatusLabel.Caption := 'Installing .NET 6.0...'
  if not Exec(ExpandConstant('{tmp}\dotnet-windows-rt-6.0.36.exe'), '/q /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    { you can interact with the user that the installation failed }
    MsgBox('.NET installation failed with code: ' + IntToStr(ResultCode) + '.',
      mbError, MB_OK);
  end;
end;

procedure InstallVCRedist;
var
  ResultCode: Integer;
begin
  WizardForm.StatusLabel.Caption := 'Installing Visual C++ 2015 redistributable...'
  if not Exec(ExpandConstant('{tmp}\vcredist.exe'), '/quiet', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    { you can interact with the user that the installation failed }
    MsgBox('Visual C++ installation failed with code: ' + IntToStr(ResultCode) + '.',
      mbError, MB_OK);
  end;
end;

function VC2015RedistNeedsInstall: Boolean;
var 
  Version: String;
begin
  if IsX86OS then
  begin
    if RegQueryStringValue(HKEY_LOCAL_MACHINE,
         'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86', 'Version',
         Version) then
    begin
      // Is the installed version at least 14.0 ? 
      Log('VC Redist (x86) Version check : found ' + Version);
      Result := (CompareStr(Version, 'v14.0.23026.00')<0);
    end
    else 
    begin
      // Not even an old version installed
      Result := True;
    end;
  end
  else
  begin
    if RegQueryStringValue(HKEY_LOCAL_MACHINE,
         'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Version',
         Version) then
    begin
      // Is the installed version at least 14.0 ? 
      Log('VC Redist (x64) Version check : found ' + Version);
      Result := (CompareStr(Version, 'v14.0.23026.00')<0);
    end
    else 
    begin
      // Not even an old version installed
      Result := True;
    end;
  end;
end;

[Icons]
; x64, arm64 - link to matrigen itself
Name: "{group}\Матрица"; Filename: "{app}\matrigen.exe"; Check: LinkToMatrigen
Name: "{commondesktop}\Матрица"; Filename: "{app}\matrigen.exe"; Tasks: desktopicon; Check: LinkToMatrigen
; x86 - link to legacylauncher
Name: "{group}\Матрица"; Filename: "{app}\leglaunch.exe"; Check: IsX86OS
Name: "{commondesktop}\Матрица"; Filename: "{app}\leglaunch.exe"; Tasks: desktopicon; Check: IsX86OS

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons"; Flags: unchecked