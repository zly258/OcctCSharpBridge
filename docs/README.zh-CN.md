# 文档索引

[English](README.md)

本目录说明可复用的 `main` 分支。完整 WinForms/WPF 应用和免配置发布流程位于 `demo` 分支。

## 建议阅读顺序

1. [快速开始](GETTING_STARTED.zh-CN.md)：环境、构建、运行时配置、Viewer、Headless 和 OCAF 第一个程序。
2. [Viewer 与显示刷新](VIEWER_AND_DISPLAY.zh-CN.md)：HWND 生命周期、相机策略、显式 Fit/FitAll、批量刷新、选择和框选。
3. [部署与运行时目录](DEPLOYMENT.zh-CN.md)：原生 DLL 查找、OCCT 资源、发布目录和再分发检查。
4. [API 覆盖说明](API_COVERAGE.md)：公共能力矩阵和有意保留的边界。
5. [OCAF/XDE 覆盖说明](OCAF_COVERAGE.md)：文档、Label、TNaming、XDE 与持久化。
6. [OCAF 扩展 API](OCAF_EXTENDED_API.md)：变量、表达式、关系式和扩展文档操作。

## 分支定位

| 分支 | 用途 |
|---|---|
| `main` | 可复用 `OcctNative`、`OcctNet`、测试和 SDK 文档 |
| `demo` | `main` + `CadCommon` + WinForms/WPF + API 中心 + 场景 + `publish.ps1` |

## 核心行为约定

- OCCT 版本固定为 7.9.0。
- 需要持有原生资源的高级对象实现 `IDisposable`。
- 原生指针不会成为公开托管 API。
- Viewer 创建 Shape 时只显示并刷新，不改变当前相机。
- `Fit`、`FitAll`、`WindowFit` 必须由业务显式调用。
- `BeginDisplayBatch()` 将多次显示修改合并为一次最终刷新。
- Headless 建模不依赖窗口。
- OCAF 修改应放入文档事务，并显式 Commit 或 Abort。
