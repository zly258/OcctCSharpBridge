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

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 all Release
```

单独构建 Demo：

```powershell
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

使用真实 OCCT Runtime 的门禁：

```powershell
.\build.ps1 smoke Release
```

启动：

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

仓库不使用 GitHub Actions 代替这些本地构建和运行验证。

## 3. 与 `main` 的手工同步

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

## 4. 文档规则

- Bridge 技术事实：只更新 `main/docs`。
- Demo 构建、启动、发布和应用层结构：更新本文件或根 README。
- 不再维护 `.md` + `.zh-CN.md` 成对的重复 SDK 文档。
- 不在 Demo 文档中复制固定 API 数量；版本和数量以 `bridge-contract.json` 与 `main` 为准。

## 5. 发布与排查

发布逻辑由 `publish.ps1` 管理。Native 启动问题优先检查：

- `OcctNative.dll`；
- OCCT `TK*.dll`；
- `native-dependencies.txt`；
- `%LOCALAPPDATA%\OcctCSharpBridge\Logs`。

Avalonia 仍通过 Windows 子 HWND 承载 Native Viewer，因此三套 Demo 都是 Windows x64 应用。
