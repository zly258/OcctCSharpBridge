# Viewer 选择与交互

`OcctEngine` 负责 AIS/Viewer 状态。正式 SDK 通过三套 UI Adapter 暴露同一套 Viewport Contract：

- `OcctNet.WinForms` — Windows x64；
- `OcctNet.Wpf` — Windows x64；
- `OcctNet.Avalonia` — Windows x64 / Linux x64。

应用层应基于托管 Viewport Contract 实现交互，不应直接处理 HWND/X11 输入。

## Viewport Host 生命周期

三套 Adapter 均实现 `IOcctViewportHost`：

```csharp
IOcctViewportHost host = viewport;

host.HostStateChanged += (_, e) => { /* Detached / Initializing / Ready / Faulted / Disposed */ };
host.Faulted += (_, e) => { /* Host 初始化或运行故障 */ };
host.EngineRecreated += (_, e) => { /* 根据 e.Engine / e.Generation 重新绑定 */ };
host.EngineDisposing += (_, e) => { /* 从当前 Engine 安全解绑 */ };
host.FirstFrameRendered += (_, e) => { /* 首个配置后的 OCCT 帧已经提交 */ };
host.NativeHandleChanged += (_, e) => { /* 仅供高级 Host 集成 */ };
```

每次 Native Host 创建新的 Engine 时，`EngineGeneration` 都会递增。需要保存 Engine 相关状态的外部服务，应通过 `EngineRecreated` / `EngineDisposing` 绑定和解绑，而不能假定一个 Viewport 生命周期中永远只有一个 Engine。

`RenderReady` 表示首个已经应用 InitialOptions 的 OCCT 帧已经提交；只有在首帧完成之后，`HostState` 才进入 `Ready`。

`NativeHandle` 表示 OCCT 实际渲染宿主：Windows 为 `HWND`，当前 Linux Backend 为 XID。它只作为高级集成与诊断逃生口存在。普通 CAD 交互应使用托管输入和 Viewport API，不应依赖 HWND/X11 细节。

## 首帧初始化配置

应在 Native Host 创建前设置 `InitialOptions`：

```csharp
viewport.InitialOptions = new OcctViewportInitializationOptions
{
    BackgroundColor = Color.FromArgb(245, 247, 250),
    ViewOrientation = OcctViewOrientation.Isometric,
    Projection = OcctProjectionType.Orthographic,
    TriedronVisible = true,
    ViewCubeVisible = true
};
```

Adapter 创建 Native Surface 时不会立即 Redraw，而是在一个 `BeginDisplayBatch()` 中完成 InitialOptions、Resize 等首帧配置，最后只提交一次首帧。Native Window 的 Map 也延迟到第一次真实 Redraw，因此 HWND/XID 真正可见时已经包含最终背景、视图、Projection 和装饰，不会先暴露空白或默认 Native Window。

Viewport 进入 Ready 后，背景、View、Projection、Triedron、ViewCube 的运行时修改继续使用普通 `OcctEngine` API。

## 平台无关 Pointer / Keyboard 输入

三套 Adapter 均实现 `IOcctViewportInputSource`，公开相同事件：

```csharp
viewport.PreviewPointerInput += (_, e) => { /* 可设置 e.Handled = true */ };
viewport.PointerInput += (_, e) => { };
viewport.PreviewKeyInput += (_, e) => { /* 可设置 e.Handled = true */ };
viewport.KeyInput += (_, e) => { };
```

公开参数只使用 Bridge 自身类型：`OcctPointerButton`、`OcctPointerButtons`、`OcctKey`、`OcctInputModifiers`，不会把 WinForms、WPF、Avalonia、Win32 或 X11 的键鼠类型暴露给应用层。

`PreviewPointerInput` / `PreviewKeyInput` 在默认 Viewport Interaction 之前触发；设置 `Handled` 可以阻止对应默认行为。这是绘制命令、夹点、捕捉、正交、动态输入、自定义导航等上层 CAD 功能的正式入口。

## Interaction Features

`OcctViewportInteractionFeatures` 替代原先过于粗粒度的默认交互总开关，可独立组合：

```csharp
viewport.InteractionFeatures =
    OcctViewportInteractionFeatures.HoverDetection |
    OcctViewportInteractionFeatures.PointSelection |
    OcctViewportInteractionFeatures.RectangleSelection |
    OcctViewportInteractionFeatures.Pan |
    OcctViewportInteractionFeatures.Zoom;
```

`Selection`、`Navigation`、`Default` 提供常用组合。

## Hover Detection

`HoverHitChanged` 只有在检测到的 Owner/Subshape 身份发生变化时才触发。同一条 Edge 或同一个 Face 内移动，即使 3D Point、Depth、DistanceToEye 持续变化，也不会形成事件风暴。

```csharp
viewport.HoverHitChanged += (_, e) =>
{
    OcctSelectionHitDetail? hit = e.Hit;
};
```

该事件复用现有 OCCT Detection 链路（`MoveTo` + detected hit detail），没有建立第二套 Native Picking 系统。

## 批量刷新

大量场景修改需要只 Redraw 一次时，统一使用已经存在的 `BeginDisplayBatch()`：

```csharp
using (engine.BeginDisplayBatch())
{
    var box = engine.MakeBox(100, 80, 60);
    engine.SetObjectColor(box, Color.SteelBlue);
    engine.SetObjectTransparency(box, 0.15);
}
```

不再新增同义的 `BeginUpdate`、`EndUpdate` 或 `DeferRefresh`。

## Edge / Face 点投影

Viewer Geometry Query 支持对真实 trimmed BRep Topology 求最近投影，并在同一次查询中直接返回捕捉/工作面需要的局部微分方向：

```csharp
var edgeProjection = engine.ProjectPointToEdge(edge, sourcePoint);
OcctPoint3d nearestOnEdge = edgeProjection.Point;
OcctVector3d tangent = edgeProjection.Tangent;
var edgePoint = engine.EvaluateEdge(edge, edgeProjection.NormalizedParameter);

var faceProjection = engine.ProjectPointToFace(face, sourcePoint);
OcctPoint3d nearestOnFace = faceProjection.Point;
OcctVector3d normal = faceProjection.Normal;
var facePoint = engine.EvaluateFace(face, faceProjection.U, faceProjection.V);
```

`OcctEdgeProjectionResult` 返回最近点、归一化 Edge 参数（`0..1`）、归一化切线和距离；`OcctFaceProjectionResult` 返回最近点、Face `U/V`、已考虑 Face Orientation 的归一化法向和距离。返回的 Parameter/UV 可通过 `EvaluateEdge` / `EvaluateFace` 回代到同一个点和方向。

这些能力可直接作为最近点、垂足、切线捕捉、面法向交互和基于面的工作平面基础，不需要上层为了获得 Tangent/Normal 再执行第二次几何查询。

## Adapter 说明

WPF 自己拥有 Native Child HWND，并把 Surface Resize 与 Redraw 分离，以合并连续布局通知。

Avalonia 属于正式 SDK。Windows 使用 HWND Host；Linux 当前使用 X11/XWayland XID Host。两套 Backend 都归一化为同一套托管 Pointer/Keyboard Contract，因此未来即使增加 Native Wayland Host，也不需要修改应用层输入 API。
