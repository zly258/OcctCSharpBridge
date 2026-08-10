# OcctCSharpBridge Demo

[English](README.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo 维护说明](docs/README.md) · [Main 技术文档](https://github.com/zly258/OcctCSharpBridge/tree/main/docs)

## 定位

`demo` 分支是 OcctCSharpBridge 的**纯二进制消费示例**。它不再包含 `main` 的 Native/Managed Bridge 源码，也不再负责 ABI、CMake、Managed Regression 或 Native Smoke。

当前只保留四个应用项目：

```text
src/OcctDemo.Common
src/OcctDemo.WinForms
src/OcctDemo.Wpf
src/OcctDemo.Avalonia
```

Bridge 通过已验证的 `dist/win-x64` Binary SDK 提供：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

### 界面预览

<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-zh.png" alt="WinForms Demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-zh.png" alt="WPF Demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/avalonia-demo-zh.png" alt="Avalonia Demo" width="88%"></p>

## 环境

- Windows x64
- .NET SDK `10.0.302`
- OCCT `7.9.0` Runtime

构建 Demo 不再需要 CMake/MSVC；只有 `main` 生成 `OcctNative.dll` 时需要 Native 工具链。

默认 OCCT 路径：

```text
D:\tools\occt-vc144-64
```

运行时也可以通过 `OCCT_ROOT` / `CASROOT` 指定其它位置。

## 1. 同步 Binary SDK

在 `main` 分支完成：

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

该脚本会先执行 Release Build、Managed Test、Native Smoke，全部成功后才刷新 `dist/win-x64`。

提交 main 的二进制后，在 `demo` 分支执行：

```powershell
.\sync-dist.ps1
```

它从 `origin/main` 同步同一路径，不需要复制整个 Bridge 项目。

## 2. 构建

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

或单独构建：

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

`build.ps1 validate` 会验证 Binary SDK 的 Contract、Manifest 和 SHA-256，并禁止 `src/OcctNative` / `src/OcctNet*` 重新进入 demo。

## 3. 运行

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

应用输出目录会自动包含 `OcctNative.dll`。`run.ps1` 负责配置 OCCT 和第三方 Runtime 搜索路径。

## 4. 发布

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

发布脚本只消费 Binary SDK，不编译 Bridge 源码。

## 项目结构

```text
dist/README.md              Binary SDK 说明
dist/win-x64/               从 main 同步的已验证 DLL/Contract/Manifest
src/OcctDemo.Common/        三套 Demo 共享应用行为
src/OcctDemo.WinForms/      WinForms Demo
src/OcctDemo.Wpf/           WPF Demo
src/OcctDemo.Avalonia/      Avalonia Demo
assets/previews/            Demo 界面预览
docs/README.md              Demo 维护规则
OcctDemo.sln                纯 Demo Solution
build.ps1                   Demo 构建入口
sync-dist.ps1               从 main 同步 Binary SDK
run.ps1                     本地运行入口
publish.ps1                 Demo 发布入口
```

## 依赖规则

- Demo 只引用 `dist/win-x64/OcctNet*.dll`，不引用 main 的 `.csproj`。
- Demo 不包含 `src/OcctNative`、`src/OcctNet*` 或 Bridge `tests`。
- 如果 Demo 调用与当前 Bridge 不一致，修改 Demo；不恢复 Legacy Alias、旧 Wrapper 或兼容层。
- Bridge 技术文档和 API/ABI 事实只在 `main` 维护。
- 仓库不使用 GitHub Actions完成构建或分支同步。

## Native 启动排查

出现 `DllNotFoundException` 或 Win32 126 时，先确认：

```text
应用目录/OcctNative.dll
%OCCT_ROOT%/win64/vc14/bin/TKernel.dll
%OCCT_ROOT%/3rdparty-vc14-64/**/bin/*.dll
```

Avalonia 仍通过 Windows 子 HWND 承载 Native Viewer，因此三套 Demo 都是 Windows x64 应用。

## 许可证

OcctCSharpBridge 使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 与其它第三方依赖遵循各自许可证。
