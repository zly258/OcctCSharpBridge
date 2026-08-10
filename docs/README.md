# Demo 分支维护说明

`demo` 分支是 **OcctCSharpBridge Binary SDK 的纯应用层消费者**。Bridge 的 Native/Managed 源码、ABI 检查、Managed Regression、Native Smoke、SDK 文档和二进制生产流程全部只在 `main` 维护。

## 1. 分支职责

```text
main
├─ src/OcctNative
├─ src/OcctNet*
├─ tests
├─ docs
├─ dist.ps1
└─ dist/win-x64        已验证 Binary SDK

demo
├─ dist/win-x64        从 main 同步的 Binary SDK
├─ src/OcctDemo.Common
├─ src/OcctDemo.WinForms
├─ src/OcctDemo.Wpf
├─ src/OcctDemo.Avalonia
├─ build.ps1
├─ run.ps1
├─ publish.ps1
└─ sync-dist.ps1
```

Demo 不包含：

- `src/OcctNative`；
- `src/OcctNet`；
- `src/OcctNet.WinForms/Wpf/Avalonia`；
- Bridge ManagedTests / Smoke；
- API/ABI/CMake PowerShell 契约脚本。

如果 Demo 调用与当前 Binary SDK 不一致，修改 Demo 调用方，不恢复 Legacy Alias、旧 Wrapper 或兼容层。

## 2. Binary SDK 同步

先在 `main` 的 Windows + MSVC + OCCT 7.9.0 环境执行：

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

只有 `all`、Managed Test 和 Native Smoke 全部成功，`dist.ps1` 才会刷新 `dist/win-x64`。

提交 main 的 `dist/win-x64` 后切换到 demo：

```powershell
.\sync-dist.ps1
```

脚本从 `origin/main` 恢复同一路径，避免复制整个 Bridge 仓库。

## 3. Binary SDK 内容

`dist/win-x64` 必须包含：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

`bridge-manifest.json` 记录版本、Native ABI、OCCT、.NET、源提交和每个分发文件的 SHA-256。`build.ps1 validate` 会验证 Contract、Manifest 和哈希。

OCCT 自身 `TK*.dll` 和第三方 Runtime 不提交到 `dist`。运行 Demo 时通过 `OCCT_ROOT` / `CASROOT` 定位 OCCT 7.9.0 Runtime。

## 4. 构建

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

`Directory.Build.targets` 会在 MSBuild 解析引用前检查 Binary SDK，并在桌面应用构建后把 `OcctNative.dll` 放到应用输出目录。Managed `OcctNet*.dll` 由正常程序集引用自动复制。

需要清理：

```powershell
.\build.ps1 clean Release
```

`clean` 只删除 Demo 自己的 `bin/obj/artifacts`，不会删除已提交的 `dist`。

## 5. 运行

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

`run.ps1` 从 `dist/win-x64/bridge-contract.json` 获取目标框架，并配置：

- `OCCT_ROOT`；
- `CASROOT`；
- `OCCT_BRIDGE_NATIVE_DIR`；
- OCCT `win64/vc14/bin`；
- `3rdparty-vc14-64/**/bin`。

## 6. 发布

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

发布脚本直接消费 Binary SDK，不再构建 Bridge。发布目录会包含 `OcctNative.dll`、Contract/Manifest 以及实际 OCCT/第三方 Runtime。

## 7. 文档规则

- Bridge API、ABI、Native、Runtime、建模与 SDK 文档：只维护 `main/docs`。
- Demo UI、构建、运行、发布：维护本文件和 demo 根 README。
- 不在 demo 重新复制一套 Bridge 技术文档。
- 不使用 GitHub Actions；Binary SDK 由维护者在真实 Windows + OCCT 环境显式验证和同步。
