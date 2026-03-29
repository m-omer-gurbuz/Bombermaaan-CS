# Installers

This folder contains the Windows installer pipeline for the C# port.

## Requirements

- .NET SDK
- Inno Setup 6 (optional if you only want the publish/release folder)

## Build

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\installers\BuildInstaller.ps1
```

The script will:

1. Read the app version from `Bombermaaan/Bombermaaan.csproj`
2. Run `dotnet publish` for `win-x64`
3. Stage a release folder under `releases/win-x64/Bombermaaan_<version>`
4. Build an Inno Setup installer if `iscc.exe` is installed

The installer output is written to `installers/output`.

