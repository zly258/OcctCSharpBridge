# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Cross-platform Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia)

The `demo` branch contains the Windows demonstration applications for the published `main` Binary SDK. It keeps the shared demo scenarios plus the two Windows UI hosts:

```text
OcctDemo.Common
OcctDemo.WinForms
OcctDemo.Wpf
```

Avalonia examples and packaging live on the `avalonia` branch, but all formal SDK assemblies are produced from `main`.

## Demo previews

### WinForms

[![WinForms demo](assets/previews/winform-demo-en.png)](assets/previews/winform-demo-en.png)

### WPF

[![WPF demo](assets/previews/wpf-demo-en.png)](assets/previews/wpf-demo-en.png)

Click a preview to open the original PNG.

## SDK consumption

`demo/dist/win-x64` is local and ignored by Git. Synchronize the currently published Windows SDK from `main`:

```powershell
.\sync.ps1
```

The synchronized distribution is the complete `main` SDK. These Windows examples reference only Core, WinForms and WPF; `OcctNet.Avalonia.dll` remains part of the synchronized SDK for contract and hash consistency.

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 all Release
```

## Run

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
```

## Publish portable demo applications

```powershell
.\publish.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Branch responsibilities

- `main`: sole formal SDK source (`OcctNative`, `OcctNet`, WinForms, WPF and Avalonia).
- `demo`: Windows WinForms/WPF demonstration applications.
- `avalonia`: Avalonia consumer examples, Windows/Linux packaging and previews.
- `website`: bilingual project website presenting both demo families.

The project uses GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0; see the repository license files.