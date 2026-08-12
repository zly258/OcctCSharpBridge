# OcctCSharpBridge Demo

[English](README.md) · [main](https://github.com/zly258/OcctCSharpBridge/tree/main) · [跨平台 Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia)

`demo` 分支只保留消费 main 已发布 Windows Binary SDK 的演示程序：

```text
OcctDemo.Common
OcctDemo.WinForms
OcctDemo.Wpf
```

Avalonia 已从 demo 移除。真正的 Windows/Linux Avalonia Host 位于独立 `avalonia` 分支。

## 同步 main Windows SDK

`demo/dist/win-x64` 只在本地存在并被 Git 忽略：

```powershell
.\sync.ps1
```

该分支只要求 Core、WinForms、WPF Bridge Assembly，不再要求 `OcctNet.Avalonia.dll`。

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

## 预览图

只保留四张 WinForms/WPF Canonical Screenshot：

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
```

## 分支职责

- `main`：Windows Bridge + 正式 Windows Binary SDK（Core、WinForms、WPF）。
- `demo`：仅 Windows WinForms/WPF Demo。
- `avalonia`：独立 `OcctNet + OcctNet.Avalonia`，同时面向 Windows x64 + Linux x64。
- `website`：公开官网。

许可证为 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。