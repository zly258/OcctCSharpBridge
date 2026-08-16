# Demo Publish Layout

Windows x64:

```text
artifacts/publish/
├─ CAD-Winform-win-x64/
├─ CAD-WPF-win-x64/
└─ CAD-Avalonia-win-x64/
```

Linux x64:

```text
artifacts/publish/
└─ CAD-Avalonia-linux-x64/
```

Each package is independent. Windows `publish.ps1 all` produces all three Windows packages. Linux `publish.sh` produces Avalonia only.
