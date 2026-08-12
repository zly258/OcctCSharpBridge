# 05 Viewer 选择与交互

本文说明 `OcctEngine` 以及 WinForms/WPF/Avalonia Viewport Host 的主要交互能力。Viewer 层只负责 OCCT 显示和交互，不实现上层 CAD Command/Tool 状态机。

## 1. 初始化与窗口生命周期

直接使用 `OcctEngine`：

```csharp
using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.Resize();
engine.Redraw();
```

`Initialize()` 需要有效 HWND。桌面项目通常直接使用对应 Host，让 Host 负责窗口创建、Resize、Mouse Capture、DPI 等 UI 框架差异。

## 2. View 与 Camera

主要能力包括：

- Isometric / Front / Back / Left / Right / Top / Bottom；
- Orthographic / Perspective；
- FitAll / FitObject / FitSelected / WindowFit；
- Zoom / ZoomAtPoint；
- Pan；
- Rotation；
- Camera Get/Set；
- View Scale；
- ScreenToWorld / WorldToScreen / ScreenToRay；
- Z-Up 方向；
- Auto Z Fit。

Viewer 状态可以通过结构化类型读取，不要求业务层自己记录一份并假定 Native 已同步。

## 3. 显示对象

`OcctEngine` 管理已注册 Viewer Object，主要类型包括：

- `OcctShape`；
- `OcctText`；
- `OcctDimension`；
- `IOcctObject` 公共对象接口。

对象 ID 只在所属 Engine 中有效。删除、更新、外观和 Transform 操作都会先验证 Owner。

## 4. 外观与渲染

当前 Viewer API 覆盖：

- Color；
- Transparency；
- Material；
- Display Mode；
- Face Boundary；
- Polygon Offset；
- Selection/Hover Highlight Color；
- Background / Gradient Background；
- Scene Lighting；
- MSAA；
- Rendering Method；
- Shadows；
- Frustum Culling；
- Render Resolution；
- Display Precision。

批量修改多个对象时优先使用已有批量入口，避免业务层高频逐对象跨 P/Invoke。

## 5. Selection Mode

Viewer 支持整对象和子拓扑选择：

```text
Object
Vertex
Edge
Wire
Face
Shell
Solid
```

Selection 操作包括 Replace、Add、Remove、Toggle、Clear 等语义。上层 Tool 可以决定何时切换模式，但具体 Command/Tool Framework 不属于 Bridge。

## 6. 结构化 Selection Hit

读取当前选中项：

```csharp
var selected = engine.GetSelectedHits();
```

读取当前 Hover/Detected：

```csharp
if (engine.TryGetDetectedHit(out var hit))
{
    Console.WriteLine($"{hit.Owner.Id}: {hit.SubshapeType} #{hit.SubshapeIndex}");
}
```

`OcctSelectionHit` 包含：

- `Owner`：所属已注册 Viewer Object；
- `SubshapeType`；
- `SubshapeIndex`；
- `IsSubshape`。

整对象选择统一使用：

```text
SubshapeType  = Shape
SubshapeIndex = -1
```

## 7. Subshape Index 的边界

`SubshapeIndex` 与当前 Root Shape 的遍历顺序对应，适合一次 Viewer 会话中的：

- Face/Edge 属性查看；
- 测量；
- 圆角/倒角输入；
- 删除 Face；
- Feature Command 参数选择。

但它不是 Persistent Naming。模型发生 Boolean、Feature、Healing 或拓扑重构后，原 Index 不能当成长期身份。

需要长期引用时，应转成 Modeling 层的 `Topology Reference` 或上层 Feature 语义引用。

## 8. Selected Hit 的 Bulk 传输

`GetSelectedHits()` 使用两次调用模式：

```text
occt_selected_hits(handle, null, 0, &count)
allocate
occt_selected_hits(handle, buffer, capacity, &filled)
```

因此选中数量增加时不会出现 `count + hit_at(index)` 的 N+1 跨 ABI 调用。

## 9. Rectangle Selection

Bridge 提供矩形框选和 Overlay 能力。上层交互策略可以根据拖拽方向决定是否允许 Overlap，但框选矩形绘制、Native Selection 与 UI Host 输入连接由 Bridge/Host 负责。

Selection State 与 Overlay 分开维护，避免“结构化选择身份”和“2D 辅助显示”耦合在同一模块。

## 10. Object Update

已有 Viewer Shape 更新能力支持在替换几何时显式选择是否保留：

- Appearance；
- Transformation；
- Selection；
- Selectability；
- Presentation/Selection Recompute。

这适合上层参数化 CAD 在 Feature 重新计算后更新 Viewer 对象，而不是简单 Delete + Recreate 导致所有交互状态丢失。

## 11. Annotation

Bridge 支持 Viewer Annotation、Text、Dimension 和 Vector Annotation。它们属于显示/交互基础设施。

以下内容仍由上层决定：

- 标注业务规则；
- 尺寸链逻辑；
- 工程图排版；
- 专业符号标准；
- 标注对象与 Document Feature 的业务绑定。

## 12. UI Host

### WinForms

`OcctViewportControl` 直接承载 Windows HWND。

### WPF

`OcctWpfViewport` 负责 WPF 与 WinForms/Native HWND 之间的 Host 适配。

### Avalonia

`OcctAvaloniaViewport` 使用 Windows 子 HWND。当前仅表示 Avalonia 应用可以在 Windows 使用该 Viewer，并不代表 OCCT Viewer 已跨平台。

## 13. 上层 CAD Tool 应如何使用 Bridge

推荐分工：

```text
Mouse / Keyboard Event
        ↓
Application Tool
        ↓
决定当前操作状态、捕捉、约束、预览策略
        ↓
OcctEngine
        ↓
Viewer Selection / Display / Transform / Redraw
```

Bridge 提供稳定的 Viewer 原语；Tool 的状态机、撤销记录、业务对象创建仍由应用控制。
