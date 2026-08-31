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

Adapter 创建 Native Surface 时使用 `redrawAfterInitialize: false` 初始化，在一个 `BeginDisplayBatch()` 中完成 InitialOptions 和初始 Resize，然后把首次真实的 `ResizeSurface() + Redraw()` 合并到 UI 调度器的 Render 阶段。Native Handle 已创建、Engine 已初始化和“首个 OCCT 帧已经提交”是三个不同状态；应用层不要把 `NativeHandle != 0` 或 `IsEngineInitialized` 当成首帧完成，应以 `RenderReady` / `FirstFrameRendered` 为准。

## 首次空白或“移动鼠标后才显示”

这是 WPF/Avalonia 托管原生 OCCT Viewport 时最常见的生命周期问题之一，并不是鼠标本身负责绘制。

WPF 的 `HwndHost` 和 Avalonia 的 `NativeControlHost` 都把 OCCT 放在独立的 Native Child Window/Surface 中。Native Handle 创建完成时，外层 UI 往往还没有结束 Measure/Arrange、DPI 同步、可见性切换或最终 Native Bounds 更新。OCCT 侧则要求在宿主尺寸变化后同步 Viewport 尺寸，并在 View 真正显示后执行一次真实 Redraw。OCCT 的 `V3d_View::MustBeResized()` 用于窗口尺寸变化，`Redraw()` 用于显式重绘；仅仅 Invalidate/创建 Handle 并不等价于提交首帧。

典型错误时序是：

1. Native Handle 创建；
2. OCCT Surface 初始化；
3. 在最终布局尺寸到达之前执行了一次 Resize/Redraw，或者根本没有在最终布局后 Redraw；
4. WPF/Avalonia 随后完成最终布局，但 Native OCCT Surface 没有再收到有效刷新；
5. Viewport 保持空白，直到后续输入、Resize、DPI 或可见性事件偶然触发下一次 Redraw。

“鼠标移动后突然显示”是这个问题非常典型的诊断特征。默认 Hover 路径会调用 `OcctEngine.MoveTo(...)`，Bridge Native Selection 随后执行 `requestRedraw()`；因此鼠标移动只是偶然补上了缺失的首帧刷新，并不是正确的初始化方式。

SDK/应用层应遵守以下规则：

- 不要在 Window/UserControl 构造函数里把 Native Handle 创建或 Engine 初始化完成当成 Viewport 已经可见；
- 首帧静态配置使用 `InitialOptions`，需要绑定 Engine 生命周期的服务使用 `EngineRecreated`；
- 首个可见帧以 `RenderReady` / `FirstFrameRendered` 为准；
- 自定义 Host 必须保证最终布局尺寸确定后执行 `ResizeSurface()`，然后执行真实 `Redraw()`；
- Size、DPI、Visible、Tab/Docking、最小化恢复、重新挂载 Native Host 等变化应走合并后的 `RefreshNativeView()` 路径，不要依赖鼠标事件、Timer 或反复调用 Redraw；
- `Invalidate` 只表示内容失效，不能替代首帧 `Redraw`；
- 125%/150% DPI 缩放通常不是根因，但会增加 Measure/Arrange、DPI 和 Native Bounds 变化次数，因此更容易暴露这个时序问题。

如果出现“启动空白，移动鼠标后正常”，可在最终布局完成后临时调用一次 `RefreshNativeView()` 诊断：如果立即恢复，基本可以确认是 Native Host 布局/首帧 Resize+Redraw 时序问题。正式修复应放在 Host 生命周期中，而不是保留这个临时调用。

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

## Engine 线程亲和性

`OcctEngine` 在成功初始化 Native Surface 时绑定当前线程，并记录当前 `SynchronizationContext`。初始化后的同步 Viewer、AIS、Scene、Selection 和 Exchange 调用必须发生在该线程；跨线程同步调用会抛出明确的 `InvalidOperationException`，而不是继续进入非线程安全的 OCCT Viewer。

`OcctEngine` 的异步导入导出方法会投递到 Surface 初始化线程，不再通过 `Task.Run` 从线程池直接操作 Viewer。此方式保证线程安全，但文件解析期间仍可能占用 UI 线程。需要真正的后台并行交换或建模时，应使用独立的 `OcctModelingSession`，完成后再在 UI 线程更新 Viewer Shape。

`OcctEngine.Dispose()` 也应在 Surface 初始化线程调用。WinForms、WPF 和 Avalonia Adapter 已在各自的 Native Host 生命周期中执行该操作。
