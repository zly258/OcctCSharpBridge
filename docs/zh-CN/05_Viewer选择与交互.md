# Viewer 选择与交互

main 中 `OcctEngine` 负责 AIS/Viewer 状态，只提供两个 Windows UI Adapter：

- `OcctNet.WinForms`
- `OcctNet.Wpf`

Viewer API 覆盖 Camera/Projection、Display Mode、Color/Material/Transparency、Transform、Text/Dimension/Point、Lighting、Redraw Batching、Selection/Detection、Rectangle Selection、Screen/World Conversion 与交互辅助。

WPF 使用 Native HWND Host，并把 Surface Resize 与 Redraw 分开，以便对连续 Resize 通知进行合并刷新。

Avalonia 不再属于 main。Windows/Linux Avalonia 使用统一 `OcctAvaloniaViewport` 的实现位于 `avalonia` 分支。