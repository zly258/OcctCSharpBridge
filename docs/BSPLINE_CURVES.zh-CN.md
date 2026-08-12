# B-Spline 曲线与曲面检查

OcctCSharpBridge 已支持插值 B-Spline 创建、微分几何求值，并提供 Edge 曲线与 Face 曲面的只读定义数据检查接口。

## 读取曲线定义

```csharp
using var model = new OcctModelingSession();

var edge = model.MakeInterpolatedBSpline(new[]
{
    new OcctPoint3d(0, 0, 0),
    new OcctPoint3d(20, 15, 5),
    new OcctPoint3d(45, -5, 12),
    new OcctPoint3d(70, 20, 18),
    new OcctPoint3d(100, 0, 25)
});

var curve = model.GetBSplineCurveData(edge);
Console.WriteLine($"Degree: {curve.Degree}");
Console.WriteLine($"Rational: {curve.IsRational}");
Console.WriteLine($"Periodic: {curve.IsPeriodic}");
Console.WriteLine($"Poles: {curve.PoleCount}");
Console.WriteLine($"Knots: {curve.KnotCount}");
```

`OcctBSplineCurveData` 包含：

- `Degree`：次数；
- `IsRational`：是否有理；
- `IsPeriodic`：是否周期；
- `Poles`：控制点；
- `Weights`：权重；
- `Knots`：不同的节点值；
- `Multiplicities`：各节点重数；
- `PoleCount`；
- `KnotCount`。

返回集合是只读快照。后续即使源 OCCT Shape 发生变化，已经取得的快照也不会被隐式修改。

## 读取曲面定义

当 Face 的底层几何为 B-Spline Surface 时：

```csharp
var surface = model.GetBSplineSurfaceData(face);

Console.WriteLine($"Degree: {surface.UDegree} x {surface.VDegree}");
Console.WriteLine($"Pole grid: {surface.UPoleCount} x {surface.VPoleCount}");
Console.WriteLine($"U knots: {surface.UKnotCount}");
Console.WriteLine($"V knots: {surface.VKnotCount}");

var pole = surface.GetPole(2, 3);
var weight = surface.GetWeight(2, 3);
```

`OcctBSplineSurfaceData` 提供：

- `UDegree`、`VDegree`；
- `IsURational`、`IsVRational`；
- `IsUPeriodic`、`IsVPeriodic`；
- `UPoleCount`、`VPoleCount`、`PoleCount`；
- 扁平只读 `Poles` 与 `Weights`；
- `UKnots`、`UMultiplicities`；
- `VKnots`、`VMultiplicities`；
- `GetPole(uIndex, vIndex)` 与 `GetWeight(uIndex, vIndex)`。

Pole / Weight 扁平数组采用 **U 主序，V 方向连续变化**：

```text
flatIndex = uIndex * VPoleCount + vIndex
```

业务代码需要二维控制网格时，建议优先调用 `GetPole()` / `GetWeight()`，不要自行计算扁平索引。

## 索引与权重约定

公开 .NET Pole/Knot 索引统一从 **0** 开始。Native Bridge 内部负责转换到 OCCT 的 1 起始索引。

对于非有理 B-Spline，OCCT 返回单位权重，因此业务层无需分别编写“有理/非有理”两套遍历逻辑。

Curve 的 `Knots`，以及 Surface 的 `UKnots` / `VKnots` 都保存不同的 Knot 值；对应的 Multiplicity 集合与 Knot 集合长度一致。

## 错误行为

`GetBSplineCurveData()` 要求输入 Shape 为 Edge，且其三维曲线必须是 B-Spline；`GetBSplineSurfaceData()` 要求输入 Shape 为 Face，且其底层曲面必须是 B-Spline。传入不兼容 Shape 时通过统一 `OcctException` Native 错误链返回，不返回半成品数据。

Managed 层还会额外拒绝：

- 非有限控制点或 Knot；
- 非正权重或 Multiplicity；
- 无效 Degree / Count 元数据；
- 非严格递增的不同 Knot 值。

## ABI 与代码组织

B-Spline 检查能力已从 generic Modeling Extensions 中拆出，独立维护在 `OcctModelingBSpline.h/.cpp`。

Curve 使用：

- `occt_model_edge_bspline_info`
- `occt_model_edge_bspline_pole_at`
- `occt_model_edge_bspline_knot_at`

Surface 使用：

- `occt_model_face_bspline_info`
- `occt_model_face_bspline_pole_at`
- `occt_model_face_bspline_u_knot_at`
- `occt_model_face_bspline_v_knot_at`

这些都是 **ABI 3 的向后兼容增量扩展**，没有删除或修改已有 ABI 3 函数签名。

## Native 验证

云端 CI 会验证 Native 声明、定义、P/Invoke 与高层接口一致性并编译 Smoke 项目；真实提取由以下 Smoke 覆盖：

- `tests/OcctNet.Smoke/BSplineSmoke.cs`
- `tests/OcctNet.Smoke/BSplineSurfaceSmoke.cs`

需要在安装 OCCT 7.9.0 的 Windows 环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```

矩阵、坐标、包围盒和拓扑快捷接口见 [Managed 几何与变换工具](GEOMETRY_UTILITIES.zh-CN.md)。
