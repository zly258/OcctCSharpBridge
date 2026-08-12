# Viewer、选择与显示刷新

## Viewer 生命周期

`OcctEngine` 管理一套 OCCT Viewer/View/InteractiveContext。有效 HWND 创建后初始化一次，宿主控件销毁时释放。

推荐顺序：

1. 创建 `OcctEngine`。
2. 等待 Win32 宿主句柄创建。
3. 调用一次 `Initialize(hwnd)`。
4. 在宿主尺寸变化时调用 `Resize()`。
5. Viewer 操作都在拥有 HWND 的 UI 线程执行。
6. 宿主窗口不再使用后释放 Engine。

## 相机策略

创建、复制、变换、导入或显示 Shape 时，不会自动调用 `FitAll`。当前眼点、目标点、上方向、投影方式、比例和用户导航状态都会保留。

需要调整视图时显式调用：

| API | 用途 |
|---|---|
| `Fit(shape)` | 适配一个显示 Shape |
| `FitAll()` | 适配全部显示对象 |
| `WindowFit(x1, y1, x2, y2)` | 适配屏幕矩形 |
| `SetView(...)` | 切换标准方向 |
| `SetCamera(...)` | 设置或恢复精确相机 |

这样可以避免每执行一个建模命令就突然重置用户视图。

## Redraw 与 Fit 的区别

场景变化和相机变化是两类操作：

- Shape 创建使用 `Display(..., false)`，只请求刷新。
- 颜色、材质、透明度、显隐和显示模式修改只请求刷新。
- `Fit`、`FitAll` 修改相机参数后刷新。
- 框选指示框使用 OCCT Immediate Layer，不需要完整场景 Fit。

## 批量显示

一个逻辑操作创建或修改多个对象时，使用 `BeginDisplayBatch()`：

```csharp
using (engine.BeginDisplayBatch())
{
    var a = engine.MakeBox(100, 80, 60);
    var b = engine.MakeCylinder(20, 80, 130, 0, 0);
    engine.SetColor(a, Color.SteelBlue);
    engine.SetColor(b, Color.OrangeRed);
}
```

最外层作用域结束时只执行一次最终 Redraw，相机保持不变。

确实需要结束时适配全部对象，可在批次内显式调用：

```csharp
using (engine.BeginDisplayBatch())
{
    // 创建和设置多个对象
    engine.FitAll();
}
```

也可以使用 `BeginDisplayBatch(fitAllOnDispose: true)`，但推荐把 `FitAll()` 显式写在业务流程中，使意图更清楚。

### 嵌套批次

批次支持嵌套。内层作用域只减少更新深度，待处理刷新或 Fit 会在最外层结束时执行。

必须使用 `using` 或确保 `Dispose()`，否则最终刷新不会正常结束。

## 选择模式

Viewer 支持：

- Object
- Vertex
- Edge
- Wire
- Face
- Shell
- Solid

点选使用屏幕坐标。矩形框选会规范化两个角点后交给 OCCT Interactive Context。

追加选择通常由宿主 UI 根据 `Ctrl` 状态控制。

## 框选指示框

矩形指示框使用 OCCT 的 `AIS_RubberBand`，显示在顶层 Immediate Layer，不再使用 Win32 XOR 反色绘制。鼠标移动时只 Redisplay 覆盖层并刷新 Immediate Layer，可避免闪烁、残留和 WPF 嵌入时的屏幕坐标问题。

视口控件公开边线颜色、填充颜色、透明度、线宽和拖动阈值配置。

## 框选状态顺序

鼠标释放捕获时，系统可能同步触发 CaptureChanged。因此必须先保存“是否形成矩形框选”的判断结果，再释放捕获，最后执行选择。

共享 `OcctViewportControl` 已实现该流程，WinForms 与 WPF 共用同一逻辑。

## 显示属性

以下操作保持对象 ID 不变：

- `SetVisible`
- `SetColor`
- `SetTransparency`
- `SetMaterial`
- `SetDisplayMode`
- `SetLineWidth`
- `Redisplay`
- `Highlight` / `Unhighlight`

需要独立选择、属性、删除和模型树节点时，应保留多个独立对象，不要为了减少 Display 次数把所有内容强行合并成 Compound。只有业务上确实把结果视为一个拓扑对象时才使用 Compound。

## 性能建议

- 多对象创建和样式修改使用批量作用域。
- 不要在循环中调用 `FitAll()`。
- 鼠标移动时避免完整 `Redraw()`；临时反馈使用 Immediate Overlay。
- 昂贵布尔、修复和网格计算可放在 Headless Session，完成后只复制最终 Shape 到 Viewer。
- 不要从多个线程并发调用同一个 Engine。
- 一个视口长期复用一个 Engine，不要每个命令重建 Viewer。

## WPF 宿主

WPF Demo 通过 `WindowsFormsHost` 复用 `OcctViewportControl`。因此选择、相机、批量刷新和框选行为与 WinForms 共用；相关修复应放在 `OcctNet`，而不是维护两套 UI 实现。

## 深度精度与共面对象

Viewer 中应区分两种机制：

- `SetAutoZFitMode()` 与 `AutoZFit()` 调整相机近、远 Z 范围，用于提高深度缓冲精度和避免裁剪，但无法区分两个深度完全相同的面。
- Polygon Offset 对指定 AIS 对象施加渲染深度偏移，适用于预览、覆盖面、参考面以及其他有意共面显示的对象。

```csharp
engine.SetAutoZFitMode(true, 1.0);
engine.AutoZFit();

var reference = engine.MakePlaneFace(100, 80);
var overlay = engine.MakePlaneFace(100, 80);

// 负值会让覆盖对象在深度上更靠近视口。
engine.SetPolygonOffsets(
    overlay,
    OcctPolygonOffsetMode.Fill,
    factor: -1.0,
    units: -1.0);

// 恢复当前 Viewer 默认值，通常为 Fill / 1 / 1。
engine.ResetPolygonOffsets(overlay);
```

不要给两个共面对象设置完全相同的自定义偏移，否则二者的深度关系仍然不明确。正式模型中的重复几何仍应删除或隐藏；Polygon Offset 用于有意的视觉分层，不用于修复无效拓扑。

