# OcctCSharpBridge Demo

[English](README.md) · [main](https://github.com/zly258/OcctCSharpBridge/tree/main) · [跨平台 Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia)

`demo` 分支用于展示 `main` 已发布 Windows Binary SDK 的实际应用效果，保留共享 Demo 场景以及两个 Windows UI Host：

```text
OcctDemo.Common
OcctDemo.WinForms
OcctDemo.Wpf
```

Avalonia 示例与打包流程位于 `avalonia` 分支，但所有正式 SDK 程序集都由 `main` 产出。

## Demo 运行预览

### WinForms

[![WinForms 中文运行界面](assets/previews/winform-demo-zh.png)](assets/previews/winform-demo-zh.png)

### WPF

[![WPF 中文运行界面](assets/previews/wpf-demo-zh.png)](assets/previews/wpf-demo-zh.png)

点击图片可查看原始 PNG 大图。

## 同步 main Windows SDK

`demo/dist/win-x64` 只在本地存在并被 Git 忽略：

```powershell
.\sync.ps1
```

同步内容是完整的 `main` SDK。本分支示例只引用 Core、WinForms、WPF；`OcctNet.Avalonia.dll` 仍随完整 SDK 同步，以保持契约和哈希一致。

## 构建

```powershell
.\build.ps1 validate Release
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 all Release
```

## 运行

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
```

## 发布 Demo

```powershell
.\publish.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

## 分支职责

- `main`：唯一正式 SDK 源（`OcctNative`、`OcctNet`、WinForms、WPF、Avalonia）。
- `demo`：Windows WinForms/WPF 演示应用。
- `avalonia`：Avalonia Consumer 示例、Windows/Linux 打包与预览。
- `website`：统一展示两组 Demo 的中英文官网。

许可证为 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。