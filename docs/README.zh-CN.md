# Demo 文档索引

[English](README.md) · [仓库 README](../README.zh-CN.md) · [可复用 SDK：`main` 分支](https://github.com/zly258/OcctCSharpBridge/tree/main)

本目录说明完整 `demo` 分支，包括可复用封装、WinForms/WPF 共同行为、可执行 API 场景、开发构建和免配置发布包生成。

## 建议阅读顺序

1. [快速开始](GETTING_STARTED.zh-CN.md)：环境、构建、运行时配置，以及 Viewer、Headless 和 OCAF 第一个程序。
2. [Viewer、选择与显示刷新](VIEWER_AND_DISPLAY.zh-CN.md)：HWND 生命周期、相机保持、显式 Fit/FitAll、批量刷新和框选。
3. [Demo 免配置发布](PUBLISHING_DEMO.zh-CN.md)：一条命令生成 WinForms/WPF 自包含发布包。
4. [部署与运行时目录](DEPLOYMENT.zh-CN.md)：原生依赖查找、OCCT 资源、干净环境测试和再分发检查。
5. [API 覆盖说明](API_COVERAGE.md)：公共能力矩阵和有意保留的 ABI 边界。
6. [OCAF/XDE 覆盖说明](OCAF_COVERAGE.md)：文档、Label、TNaming、XDE 和持久化。
7. [OCAF 扩展 API](OCAF_EXTENDED_API.md)：变量、表达式、关系式和扩展文档操作。

## Demo 专用组件

| 组件 | 用途 |
|---|---|
| `CadCommon` | 共享命令、会话状态、示例、国际化、撤销重做和 API 场景 |
| `CadWinForms` | WinForms 宿主和桌面界面 |
| `CadWpf` | WPF 宿主，复用共享 OCCT 视口 |
| API 中心 | 基于反射的全部 `OcctNet` 公开成员目录和可执行综合场景 |
| `publish.ps1` | 带原生运行库和资源的 Windows x64 自包含发布包生成 |

## 核心行为约定

- OCCT 版本固定为 7.9.0。
- Viewer 创建 Shape 时只显示并刷新，不改变当前相机。
- `Fit`、`FitAll` 和 `WindowFit` 必须显式调用。
- 多对象示例使用 `BeginDisplayBatch()`，最多进行一次最终刷新。
- WinForms 与 WPF 共用 `OcctViewportControl`，选择和框选修复统一放在 `OcctNet`。
- 公共 API 通过反射自动进入目录，可发现性与是否能自动执行相互分离。
- Headless 建模不依赖窗口。
- OCAF 修改应放入文档事务，并显式 Commit 或 Abort。
- 免配置发布包必须在干净 Windows x64 环境验证后再分发。

## 构建、运行与发布

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"

.\run.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"

.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

开发命令需要 OCCT SDK。生成的自包含发布包面向没有开发环境、没有 OCCT 配置的使用者。
