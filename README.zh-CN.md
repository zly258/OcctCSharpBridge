# OcctCSharpBridge Demo

[English](README.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` 是唯一的 Binary SDK Consumer 分支，`main` 是唯一的 Bridge SDK 源。

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

平台矩阵：

| 平台 | WinForms | WPF | Avalonia |
|---|---:|---:|---:|
| Windows x64 | 是 | 是 | 是 |
| Linux x64 | 否 | 否 | 是 |

Demo 严格作为 Bridge 3 / ABI5 Consumer：不跟踪 `OcctNative`、`OcctNet*` 实现源码，也不直接调用 `occt_*` Native ABI。

## Binary SDK 流程

`dist/` 只作为本地构建状态存在并被 Git 忽略。Windows/Linux 两套同步脚本都会校验 contract schema 3、manifest schema 2、ABI5-only、.NET SDK 10.0.303、C# 14 和 SDK 文件哈希；当 `manifest.sourceCommit` 已与 `origin/main` 一致时直接复用，不重复构建 SDK。

Windows：

```powershell
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux：

```bash
./sync.sh
./build.sh all Release
./run.sh Release
./publish.sh Release
```

Linux 只构建 `OcctDemo.Common` 和 `OcctDemo.Avalonia`，绝不构建 WinForms/WPF。当前 Avalonia Viewer 交互运行需要 X11/XWayland。

平台细节见 [LINUX.md](LINUX.md) 和 [docs/platform-matrix.md](docs/platform-matrix.md)。

## Demo 预览

- WinForms / Windows：`assets/previews/winform-demo-zh.png`
- WPF / Windows：`assets/previews/wpf-demo-zh.png`
- Avalonia / Windows：`assets/previews/avalonia-win-demo-zh.png`
- Avalonia / Linux：`assets/previews/avalonia-linux-demo-zh.png`

## 分支职责

- `main` / `main-dev`：Bridge SDK 源码和开发。
- `demo` / `demo-dev`：统一的 Windows/Linux Demo Consumer。
- `website`：中英文项目官网。
- `backup/*`：历史备份，本次迁移不修改。

原独立 `avalonia`、`avalonia-dev` 分支在有效内容迁入 `demo` 后废弃。

许可证为 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。
