# B-Spline 曲线检查

OcctCSharpBridge 已支持插值 B-Spline 创建、微分几何求值，并新增 `OcctModelingSession.GetBSplineCurveData()`，用于完整读取 B-Spline 曲线的定义数据。

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

var data = model.GetBSplineCurveData(edge);

Console.WriteLine($"Degree: {data.Degree}");
Console.WriteLine($"Rational: {data.IsRational}");
Console.WriteLine($"Periodic: {data.IsPeriodic}");
Console.WriteLine($"Poles: {data.PoleCount}");
Console.WriteLine($"Knots: {data.KnotCount}");
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

## 索引与权重约定

公开 .NET 集合统一使用 **0 起始索引**。Native Bridge 内部负责转换到 OCCT 的 1 起始 Pole/Knot 索引。

`Weights.Count` 始终与 `Poles.Count` 一致。对于非有理 B-Spline，OCCT 返回单位权重，因此业务层无需分别编写“有理/非有理”两套遍历逻辑。

`Knots` 保存不同的节点值；`Multiplicities[index]` 对应 `Knots[index]` 的节点重数，两者长度始终一致。

## 错误行为

`GetBSplineCurveData()` 要求输入 Shape 为 Edge，且其三维曲线必须是 B-Spline。若传入 Line、Circle 或其他非 B-Spline Edge，通过统一 `OcctException` Native 错误链返回，而不是返回不完整数据。

Managed 层还会额外拒绝异常结果，包括：

- 非有限控制点或节点值；
- 非正权重；
- 非正节点重数；
- 非严格递增的不同 Knot 值。

## ABI 设计

高层快照只由 3 个紧凑 C ABI 支撑：

- `occt_model_edge_bspline_info`
- `occt_model_edge_bspline_pole_at`
- `occt_model_edge_bspline_knot_at`

这是 **ABI 3 的向后兼容增量扩展**，没有删除或修改已有 ABI 3 函数签名。

## Native 验证

云端 CI 会验证 Native 声明、定义、P/Invoke 与高层接口的一致性，并编译 Smoke 项目；但云端没有本项目真实 OCCT SDK，因此真正的 B-Spline 提取由 `tests/OcctNet.Smoke/BSplineSmoke.cs` 覆盖，并需在安装 OCCT 7.9.0 的 Windows 环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```
