# Demo 分支维护说明

`demo` 分支只维护 **OcctCSharpBridge 的应用层示例、运行/发布脚本、界面预览和 Demo 专属约定**。Bridge 本身的架构、API Coverage、B-Spline、Topology、Mesh、Runtime 等技术文档统一以 [`main/docs`](https://github.com/zly258/OcctCSharpBridge/tree/main/docs) 为准，不在本分支复制第二套正文。

## 1. 分支职责

```text
main
└─ reusable Bridge / UI Hosts / tests / SDK docs / packages

demo
├─ shared Bridge source mirror
├─ OcctDemo.Common
├─ OcctDemo.WinForms
├─ OcctDemo.Wpf
├─ OcctDemo.Avalonia
├─ run.ps1 / publish.ps1
└─ assets/previews
```

应用层可以实现 Document、Command、Tool、History、对象树、属性面板和业务交互，但这些能力不反向下沉到 `main` Bridge。

## 2. 本地构建

`build.ps1` 使用增量构建；正常开发不要主动删除 `bin/obj`。只有怀疑生成缓存污染时才使用 `clean`。

基础检查和全部 Managed 项目编译：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

完整编译 Native、Bridge、三套 Demo、ManagedTests 和 Smoke：

```powershell
.\build.ps1 all Release
```

`all` 会把 `OcctNative.dll`、OCCT DLL 和第三方 Runtime 部署到 WinForms、WPF、Avalonia 与 Smoke 输出目录，使构建产物可以直接进入运行验证。

单独构建 Demo：

```powershell
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

真实 OCCT Runtime 门禁：

```powershell
.\build.ps1 smoke Release
```

启动：

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

需要清理时：

```powershell
.\build.ps1 clean Release
.\build.ps1 all Release
```

仓库不使用 GitHub Actions 代替这些本地构建、测试和运行验证。

## 3. 测试与静态检查

Demo 只保留五个稳定 PowerShell 契约：版本、Demo 结构、Bulk ABI、Native CMake 结构、API Surface。README 标题、UI 代码组织、具体函数文本等不再作为契约。

Managed 测试使用 .NET 10 的 Microsoft Testing Platform；Runner 由根目录 `global.json` 统一指定。Smoke 使用真实 `OcctNative.dll` 和 OCCT Runtime。

## 4. 与 `main` 的手工同步

以下共享内容在 `main` 完成本地验证后手工同步到 `demo`：

```text
.editorconfig
.gitattributes
global.json
bridge-contract.json
Directory.Build.props
src/OcctNative/**
src/OcctNet/**/*.cs
src/OcctNet.WinForms/**/*.cs
src/OcctNet.Wpf/**/*.cs
src/OcctNet.Avalonia/**/*.cs
```

以下内容保持分支专属，不做整个目录覆盖：

```text
README*
docs/**
tests/**
build.ps1
*.csproj / package policy
src/OcctDemo.*/**
run.ps1
publish.ps1
assets/**
```

共享源码发生冲突时，以 `main` 的 Bridge 设计为准，修改 Demo 调用方；不要为了兼容 Demo 的旧调用重新增加 Legacy Alias、旧 Wrapper 或已删除的内部聚合头。

## 5. 文档规则

- Bridge 技术事实：只更新 `main/docs`。
- Demo 构建、启动、发布和应用层结构：更新本文件或根 README。
- 不再维护 `.md` + `.zh-CN.md` 成对的重复 SDK 文档。
- 不在 Demo 文档中复制固定 API 数量；版本和数量以 `bridge-contract.json` 与 `main` 为准。

## 6. 发布与排查

发布逻辑由 `publish.ps1` 管理。Native 启动问题优先检查：

- `OcctNative.dll`；
- OCCT `TK*.dll`；
- 第三方 Runtime DLL；
- `native-dependencies.txt`；
- `%LOCALAPPDATA%\OcctCSharpBridge\Logs`。

Avalonia 仍通过 Windows 子 HWND 承载 Native Viewer，因此三套 Demo 都是 Windows x64 应用。
