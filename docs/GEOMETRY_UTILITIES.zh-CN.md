# Managed 几何与变换工具

`OcctGeometryExtensions` 提供一组**纯 Managed** 几何辅助能力，不需要调用 OCCT Native。适用于 CAD/BIM 应用在进入几何内核前完成轻量级向量计算、包围盒判断、坐标变换和拓扑遍历组织。

## 约定

- 长度单位由上层应用决定，但必须与传入 OCCT 的模型单位保持一致。
- 角度统一使用**弧度**。
- `OcctModelLocation` 与 `OcctTransform3d` 使用行优先仿射矩阵，并按列向量语义计算。
- `left.Multiply(right)` 表示先执行 `right`，再执行 `left`。
- 点变换包含平移，向量变换不包含平移。
- `OcctModelLocation` 的工具方法要求最后一行为 `[0, 0, 0, 1]`，与 Native Bridge 中 `gp_Trsf` 的映射保持一致。

## 点与向量

```csharp
var a = new OcctPoint3d(0, 0, 0);
var b = new OcctPoint3d(100, 50, 0);
var midpoint = a.Lerp(b, 0.5);

var direction = new OcctVector3d(1, 1, 0);
var angle = direction.AngleTo(OcctVector3d.UnitX);
var xComponent = direction.ProjectOnto(OcctVector3d.UnitX);
var perpendicular = direction.RejectFrom(OcctVector3d.UnitX);
```

点和向量均提供 `AlmostEquals()`，用于带容差的几何比较，避免业务代码依赖浮点数完全相等。

## AABB 包围盒

```csharp
var bounds = model.GetShapeBounds(shape);

if (bounds.IsValid() && bounds.Contains(testPoint, tolerance: 1e-6))
{
    var volume = bounds.GetVolume();
    var diagonal = bounds.GetDiagonalLength();
}

var padded = bounds.Expanded(5.0);
var combined = bounds.Union(otherBounds);
var overlaps = bounds.Intersects(otherBounds, tolerance: 1e-6);
```

这些方法只针对轴对齐 `OcctBounds`，不能替代 OCCT 精确碰撞、干涉或 Shape 距离计算。

## UV 与距离结果

```csharp
var uv = model.GetFaceUvBounds(face);
var center = uv.GetCenter();
var insideParameterRange = uv.Contains(center.U, center.V);

var distance = model.GetShapeDistance(first, second);
var separation = distance.GetSeparationVector();
var midpoint = distance.GetMidpoint();
var touching = distance.IsWithin(1e-6);
```

## Location 与对象变换

无需 Native 调用即可构造平移、旋转和均匀缩放：

```csharp
var move = OcctGeometryExtensions.CreateTranslationLocation(100, 0, 0);
var rotate = OcctGeometryExtensions.CreateRotationLocation(
    OcctVector3d.UnitZ,
    Math.PI / 2,
    center: OcctPoint3d.Origin);

var transform = move.Multiply(rotate); // 先旋转，再平移
var worldPoint = transform.TransformPoint(localPoint);
var worldDirection = transform.TransformVector(localDirection);

var localPointAgain = transform.Inverted().TransformPoint(worldPoint);
```

Viewer/Object 使用的 `OcctTransform3d` 与 Headless Modeling 使用的 `OcctModelLocation` 可以显式互转：

```csharp
OcctTransform3d viewerTransform = transform.ToTransform3d();
OcctModelLocation modelLocation = viewerTransform.ToModelLocation();
```

这样可在不修改 Native ABI 的情况下统一两套已有公开变换类型的使用方式。

## 拓扑快捷接口

通用接口仍然保留：

```csharp
var faces = model.GetSubshapes(shape, OcctShapeType.Face);
```

常见场景可直接使用快捷接口：

```csharp
var vertices = model.GetVertices(shape);
var edges = model.GetEdges(shape);
var faces = model.GetFaces(shape);
var solids = model.GetSolids(shape);

var faceEdges = model.GetFaceEdges(face);
var wireEdges = model.GetWireEdges(wire);
var edgeVertices = model.GetEdgeVertices(edge);
var counts = model.GetTopologyCounts(shape);
```

返回的 `OcctModelShape` 仍绑定其创建时的 `OcctModelingSession`，跨 Session 所有权检查规则不变。

## 哪些情况仍应使用 Native OCCT

当结果依赖精确 B-Rep 几何或拓扑时，应继续使用 Native-backed API，例如：

- Shape 间精确距离及最近点；
- 点到曲线/曲面的投影；
- 射线相交与 Solid 点分类；
- Boolean 与特征建模；
- 精确曲线/曲面求值；
- 拓扑身份与祖先关系；
- 三角网格与工程文件交换。

Managed 工具的定位是围绕 Bridge 现有值类型提供确定性、轻量级的应用层计算能力，而不是重新实现一套几何内核。
