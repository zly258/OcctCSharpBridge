# 架构边界：Bridge 与 CAD 应用层

OcctCSharpBridge 的 `main` 只负责 **Open CASCADE Technology 7.9.0 的 Native/C# 封装与可复用视口宿主**。完整 CAD 软件需要的文档、命令、交互工具、业务对象与持久化属于 `demo` 或其它上层应用，不进入 Bridge。

## `main` 应该包含什么

`main` 的职责限定为：

- `OcctNative`：OCCT C++17 Bridge 与稳定 C ABI；
- `OcctNet`：类型安全的 `OcctEngine`、`OcctModelingSession`、值类型、Runtime 与诊断；
- `OcctNet.WinForms`：WinForms HWND 视口宿主；
- `OcctNet.Wpf`：基于 `WindowsFormsHost` 的 WPF 视口宿主；
- `OcctNet.Avalonia`：基于 `NativeControlHost` + Windows HWND 的 Avalonia 视口宿主；
- 契约检查、Managed 回归测试、Native Smoke 场景和 NuGet 打包规则。

UI Host 只负责把框架窗口/鼠标事件连接到 `OcctEngine`。它们不是 CAD 应用框架。

## `main` 不应该包含什么

以下概念不属于 OCCT Bridge：

- Document / DocumentManager；
- Entity / Feature Tree 等业务模型；
- Command / CommandBus / CommandRegistry；
- Tool / ToolManager / Grip / Snap 规则；
- Undo/Redo 业务事务；
- JSON 项目文件与应用级持久化；
- Ribbon、属性面板、最近文件、工作区等产品 UI；
- BIM/设备专业属性和业务规则。

这些能力通常是完整 CAD 软件所必需的，但它们依赖具体产品的数据模型与交互规则。如果放入 Bridge，会让 OCCT 封装与业务框架相互耦合，降低其它项目复用能力。

## `demo` 的职责

`demo` 在同一套 Bridge 源码上增加参考 CAD 应用层，例如：

- `CadCommon`：简单命令目录/分发、参数解析、历史记录和 Demo 公共逻辑；
- `CadWinForms`、`CadWpf`、`CadAvalonia`：完整可运行示例；
- Demo 的运行、发布、Native Runtime 部署与应用打包脚本。

`demo` 可以展示 Document、Command、Tool、Undo/Redo 等 CAD 设计方法，但这些实现是**参考应用代码**，不是 `OcctNet` 公共 API。

## 三个核心层次

```text
CAD / BIM Application                     demo 或外部项目
├─ Document / Feature / Entity
├─ Command / Tool / Snap / Grip
├─ Undo / Redo / Persistence
└─ Product UI
              │
              ▼
Reusable .NET Bridge                      main
├─ OcctEngine
├─ OcctModelingSession
├─ WinForms / WPF / Avalonia Viewport Host
└─ Runtime / Diagnostics / Value Types
              │
              ▼
Stable C ABI                              main
              │
              ▼
Open CASCADE Technology 7.9.0
```

## `OcctEngine` 与 `OcctModelingSession`

两者有意保留部分几何构造能力重叠，但职责不同：

- `OcctEngine` 面向已初始化 Viewer，创建并管理可显示、可选择的 AIS 对象；
- `OcctModelingSession` 面向无界面几何建模、批处理、分析与文件交换。

不要为了消除表面重复而合并为一个巨型 façade。

## UI Host 的共享边界

WinForms、WPF、Avalonia 的窗口生命周期和输入 Capture 机制不同，因此不建立统一 UI 基类。只共享不依赖 UI 框架的视口交互判定，例如：

- Hover/WorldPoint 节流周期；
- 框选阈值；
- 左右方向框选是否允许 Overlap；
- 选择拖拽终点恢复；
- 默认缩放倍率。

窗口创建、DPI、Mouse Capture、Win32 子类化等仍由各 Host 自己处理，避免为了 DRY 产生脆弱的跨框架抽象。

## Avalonia 的定位

`OcctNet.Avalonia` 是正式的可复用 Host，但当前仍是 **Windows x64 + HWND** 适配器。它不表示整个 Bridge 已支持 Linux/macOS。真正跨平台需要为 OCCT Viewer 的原生窗口/图形上下文实现不同平台后端，属于单独的架构工作。

## 兼容性原则

Bridge 2.x 不为了内部整理破坏已有公开 API：

- Native ABI 3 的已有函数签名保持不变；
- `OcctObject` 等 Bridge 2.5 兼容入口暂时保留，但不继续扩展新的 legacy API；
- 新代码优先使用 owner-aware 对象接口；
- 内部 cpp 拆分、Header 收口、Host 规则去重不构成 ABI 变化。

真正删除兼容入口应放到未来明确的 major 版本中一次完成。
