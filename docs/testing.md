# Demo Validation Commands

Windows x64:

```powershell
.\sync.ps1
.\build.ps1 validate Release
.\build.ps1 all Release
```

Linux x64:

```bash
./sync.sh
./build.sh validate Release
./build.sh all Release
```

Windows validates WinForms, WPF and Avalonia. Linux validates Avalonia only.
