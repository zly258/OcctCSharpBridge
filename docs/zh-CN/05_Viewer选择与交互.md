# 05 Viewer 选择与交互

`OcctEngine` 管理 AIS Interactive Context、View/Camera 和显示对象。

## 选择

Bridge 支持点选、框选、Selection Operation、Selection Mode、Selected/Detected 结构化身份、Selectability 与应用驱动的 Selection Set。上层可关闭默认交互，接管 Raw Input，实现自己的 CAD Tool、捕捉、动态预览和工作平面。

## 显示

Viewer API 包含 Display Mode、Material、Color、Transparency、Line Width、Local Transform、Dimension、Text、View Cube、Lighting、Background、Camera/Projection 与 Display Batch。

## 一等 Point

`OcctPoint` 对应真实的 `AIS_Point + Geom_CartesianPoint`，`OcctPointMarker` 对应 OCCT 标准 Marker。位置/样式修改使用 Redisplay 请求并允许现有 Display Batch 合并刷新，不需要用 BRep Vertex 或 UI 图元模拟捕捉点/夹点。

## WPF Resize

WPF Host 使用专门的“不 redraw 的 Native surface resize”。窗口 Resize 只合并调度到 `DispatcherPriority.Render`，`WM_PAINT` 不再直接调用 OCCT Redraw，从而减少冗余刷新与缩放闪烁。

WinForms/WPF/Avalonia 保持独立 Adapter，但共享同一 `OcctEngine` 行为语义。
