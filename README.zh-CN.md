# OcctCSharpBridge Demo

[English](README.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo 维护说明](docs/README.md) · [Main 中文文档](https://github.com/zly258/OcctCSharpBridge/tree/main/docs/zh-CN)

`demo` 分支是 OcctCSharpBridge 的**纯 Binary SDK 消费示例**。它不包含 `main` 的 Native/Managed Bridge 源码，也不负责 ABI、CMake、Managed Regression 或 Native Smoke。

当前应用项目：

```text
src/OcctDemo.Common
src/OcctDemo.WinForms
src/OcctDemo.Wpf
src/OcctDemo.Avalonia
```

Bridge 由 `main/publish.ps1` 发布到本分支的 `dist/win-x64`：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

## 环境

- Windows x64
- .NET SDK `10.0.302`
- OCCT `7.9.0` Runtime

Demo 构建不需要 CMake/MSVC；Native 工具链只属于 `main` 的 Bridge 生产流程。

默认 OCCT 路径：

```text
D:\tools\occt-vc144-64
```

## Binary SDK 更新

Demo 不再维护反向同步脚本。Bridge 发布统一从 `main` 发起：

```powershell
# main branch
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

main 会完成 Release Build、Managed Test、Native Smoke、Binary SDK Manifest/SHA-256，然后使用临时 worktree 将 `dist/win-x64` 发布到 demo。

## 构建

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

单独构建：

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

`build.ps1 validate` 校验 Binary SDK Contract、Manifest 和 SHA-256，并禁止 Bridge 源码重新进入 demo。

## 运行

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

应用输出目录自动包含 `OcctNative.dll`；`run.ps1` 配置 OCCT 和第三方 Runtime 搜索路径。

## 发布 Demo 应用

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

这里的 demo `publish.ps1` 只负责发布 Demo 应用，直接消费 `dist/win-x64`，不编译 Bridge 源码。

## 项目结构

```text
dist/win-x64/               main 发布的已验证 Binary SDK
src/OcctDemo.Common/        共享应用行为
src/OcctDemo.WinForms/      WinForms Demo
src/OcctDemo.Wpf/           WPF Demo
src/OcctDemo.Avalonia/      Avalonia Demo
assets/previews/            Demo 界面预览
docs/README.md              Demo 维护规则
OcctDemo.sln                Demo-only Solution
build.ps1                   Demo 构建入口
run.ps1                     本地运行入口
publish.ps1                 Demo 应用发布入口
```

## 依赖规则

- Demo 只引用 `dist/win-x64/OcctNet*.dll`，不引用 main `.csproj`；
- Demo 不包含 `src/OcctNative`、`src/OcctNet*` 或 Bridge tests；
- Demo 调用与新 SDK 不一致时修改 Demo，不恢复 Legacy Alias 或旧 Wrapper；
- Bridge 技术文档与完整中英文 API Reference 统一维护在 `main/docs/zh-CN`、`main/docs/en-US`；
- 不使用 GitHub Actions 完成构建或分支同步。

## Native 启动排查

出现 `DllNotFoundException` 或 Win32 126 时检查：

```text
应用目录/OcctNative.dll
%OCCT_ROOT%/win64/vc14/bin/TKernel.dll
%OCCT_ROOT%/3rdparty-vc14-64/**/bin/*.dll
```

Avalonia 仍通过 Windows 子 HWND 承载 Native Viewer，因此三套 Demo 都是 Windows x64 应用。

## 许可证

OcctCSharpBridge 使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。
