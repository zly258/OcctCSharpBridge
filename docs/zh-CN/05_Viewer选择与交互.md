# Viewer 选择与交互

`main` 中 `OcctEngine` 负责 AIS/Viewer 状态，正式 SDK 提供三个 UI Adapter：

- `OcctNet.WinForms` — Windows x64；
- `OcctNet.Wpf` — Windows x64；
- `OcctNet.Avalonia` — Windows x64 / Linux x64。

Viewer API 覆盖 Camera/Projection、Display Mode、Color/Material/Transparency、Transform、Text/Dimension/Point、Lighting、Redraw Batching、Selection/Detection、Rectangle Selection、Screen/World Conversion 与交互辅助。

WPF 使用 Native HWND Host，并把 Surface Resize 与 Redraw 分开，以便对连续 Resize 通知进行合并刷新。

Avalonia 属于正式 `main` SDK。Windows 使用 Windows Native Viewer Host；Linux 使用 X11/XWayland Backend。统一 `demo` 分支在两个平台都提供 Avalonia Host，而 WinForms/WPF 保持 Windows-only。
