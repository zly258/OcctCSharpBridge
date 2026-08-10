# Demo 分支维护说明

`demo` 分支是 **OcctCSharpBridge Binary SDK 的纯应用层消费者**。Bridge Native/Managed 源码、ABI 检查、Managed Regression、Native Smoke、完整中英文 SDK 文档和 Binary SDK 生产流程全部只在 `main` 维护。

## 1. 分支职责

```text
main
├─ src/OcctNative
├─ src/OcctNet*
├─ tests
├─ tools/OcctApiDocsGenerator
├─ docs/zh-CN
├─ docs/en-US
├─ build.ps1
├─ publish.ps1
└─ dist/win-x64        已验证 Binary SDK

demo
├─ dist/win-x64        main 发布的 Binary SDK
├─ src/OcctDemo.Common
├─ src/OcctDemo.WinForms
├─ src/OcctDemo.Wpf
├─ src/OcctDemo.Avalonia
├─ build.ps1
├─ run.ps1
└─ publish.ps1         Demo 应用发布
```

Demo 不包含：

- `src/OcctNative`；
- `src/OcctNet*`；
- Bridge ManagedTests / Smoke；
- API/ABI/CMake PowerShell 契约脚本；
- main 的 Bridge 技术文档副本。

## 2. Binary SDK 发布方向

同步只允许从 `main` 发起，不在 demo 维护反向同步脚本：

```powershell
# main branch
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`main/publish.ps1` 会先运行 `build.ps1 dist Release`，完成真实 Release Build、Managed Test、Native Smoke，再校验 Contract/Manifest/SHA-256，并通过临时 detached worktree 更新 demo 的 `dist/win-x64`。

## 3. Binary SDK 内容

必须包含：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

`build.ps1 validate` 会检查 Contract、Manifest、平台/目标框架以及各文件 SHA-256。

OCCT 自身 `TK*.dll` 和第三方 Runtime 不提交到 `dist`。运行时通过 `OCCT_ROOT` / `CASROOT` 定位 OCCT 7.9.0 Runtime。

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

`Directory.Build.targets` 在 MSBuild 解析引用前检查 Binary SDK，并在桌面应用构建后把 `OcctNative.dll` 放到应用输出目录；Managed `OcctNet*.dll` 由程序集引用自动复制。

## 5. 运行与应用发布

```powershell
.\run.ps1 wpf Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Demo 的 `publish.ps1` 只发布应用程序，不发布 Bridge，也不编译 Bridge 源码。

## 6. 文档规则

- 中文 Bridge 文档：`main/docs/zh-CN`；
- 英文 Bridge 文档：`main/docs/en-US`；
- 完整 API Reference：两套语言目录下的 `api/`；
- Demo 只维护 UI、构建、运行和应用发布说明；
- 不使用 GitHub Actions 完成构建或分支同步。
