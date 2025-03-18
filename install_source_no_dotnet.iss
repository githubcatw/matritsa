; Сборка варианта установщика Матрицы без дистрибутива .NET.

[Setup]
AppName=Матрица
AppVersion=1.0-beta3
WizardStyle=modern
DefaultDirName={autopf}\Matritsa
DefaultGroupName=Матрица
UninstallDisplayIcon={app}\matrigen.exe
Compression=lzma2
SolidCompression=yes
OutputDir=userdocs:Inno Setup Output
OutputBaseFilename=setup-matritsa-no-dotnet

[Files]
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\matrigen.exe"; DestDir: "{app}"
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\*.dll"; DestDir: "{app}"
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\*.json"; DestDir: "{app}"
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win\*"; DestDir: "{app}\runtimes\win"; Flags: recursesubdirs
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win-arm64\*"; DestDir: "{app}\runtimes\win-arm64"; Flags: recursesubdirs
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win-x64\*"; DestDir: "{app}\runtimes\win-x64"; Flags: recursesubdirs
Source: "matritsa\matritsa.Desktop\bin\Release\net6.0\runtimes\win-x86\*"; DestDir: "{app}\runtimes\win-x86"; Flags: recursesubdirs
; add legacy launcher
Source: "leglaunch\leglaunch\bin\Release\leglaunch.exe"; DestDir: "{app}"; Check: IsX86OS

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Code]
function LinkToMatrigen: Boolean;
begin
  Result := IsX64OS or IsArm64
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