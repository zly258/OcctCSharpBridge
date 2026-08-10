# 结构化几何求交设计

## 目标

为 `OcctModelingSession` 增加适合 CAD 的几何求交能力，不把求交简化成一个 Boolean，也不再引入 N+1 Native 调用。

首批建议实现有拓扑边界的 Edge/Edge 求交；后续再按相同结果模型扩展 Edge/Face 与纯 Curve/Surface。

## 为什么不能只有 `bool Intersects()`

CAD 上层真正需要的是求交证据：

- 交点坐标；
- 两条参与曲线上的参数；
- 共线/重合 Edge 的重合参数区间；
- Point 与 Overlap 的结果类型；
- 求交所使用的 Tolerance。

两个 Edge 可能单点相交、端点接触、区间重合，或者产生多个 Common Part。只返回 true/false 会丢失 Snap、Trim、Constraint、Measurement、拓扑重建所需的信息。

## OCCT 映射

有拓扑边界的 Edge/Edge 求交优先使用 OCCT `IntTools_EdgeEdge` 及其 `IntTools_CommonPrt` 结果，而不是只对底层无限 Curve 求交。

Common Part 可以表达 Vertex 类的点结果，也可以表达 Edge 类的重合区间；Bridge 必须保留这种区别。

Curve/Surface 应作为单独接口增加。OCCT 的 Curve/Surface 求交也可能产生 Point 和 Segment，Bridge 不能强制全部转成 Point DTO。

## 建议的 Public Value

```text
OcctIntersectionKind
- Point
- Overlap

OcctEdgeIntersection
- Kind
- Point
- FirstParameterStart
- FirstParameterEnd
- SecondParameterStart
- SecondParameterEnd
```

对于 `Point`，Start/End 参数在 Tolerance 内应相等；对于 `Overlap`，两组 Range 表示两个输入 Edge 上对应的公共区间。

不要通过 C ABI 暴露原始 OCCT `IntTools_CommonPrt` 对象。

## Native ABI

第一版就按 Bulk 设计：

```text
occt_model_intersect_edges(...)
  -> 在所属 ModelingSession 中计算并保存当前结果集
  -> 成功返回结果数量，失败返回 -1

occt_model_edge_intersections_copy(..., buffer, capacity)
  -> 一次 Native 调用复制完整结果集
```

这与当前 Selected Hit、Ray Hit、Operation History 的批量策略一致。不要新增 Managed `for` 循环逐项调用 `intersection_at(index)` 的主路径。

因为这是全新的 API，没有兼容包袱，第一版可以直接从 Bulk-only ABI 开始，不需要为了形式完整再增加 Indexed Native Export。

## Native 模块

新增独立：

`OcctModelingIntersection.cpp`

不要再次扩大 `OcctModelingAnalysis.cpp`。后者已明确限定为 Projection、Ray Intersection、Point Classification。

Session Internal 可以像 Ray Result 一样保存一个 blittable intersection record vector。它只属于当前 `OcctModelingSession` 的临时计算状态，不是 Persistent Topology Identity。

## Edge/Edge 算法

1. Managed 层先确保两个 Shape 都属于当前 ModelingSession；
2. Native 要求两个输入都是 `TopAbs_EDGE`；
3. 用显式 Tolerance/Fuzzy Value 执行 `IntTools_EdgeEdge`；
4. 遍历全部 `IntTools_CommonPrt`；
5. Vertex 类 Common Part 转为 `Point`；
6. Edge 类 Common Part 转为 `Overlap`，保留两个输入 Edge 上的参数区间；
7. 按第一个 Edge 的参数范围进行确定性排序；
8. 只有在 Tolerance 内真正等价时才去重，不能合并不同重合区间；
9. 通过一次 Bulk ABI 返回全部结果。

## 参数语义

结果使用输入 Edge 底层 Curve 的**原生参数**，不是归一化 0..1 参数。这对精确 Trim 和后续 OCCT 运算更有价值。

如果上层需要 0..1，可以以后增加 Managed Convenience Helper，不改变 Exact Native Result。

## Tolerance

调用方必须能显式传入非负 Intersection Tolerance。Bridge 不应静默隐藏一个很大的 Fuzzy Value。

应用如果需要更严格的工程校核，可以使用自己的精度策略。

## Edge/Face 后续

Edge/Face 至少应保留：

- Point；
- Edge Parameter；
- Face `u/v`；
- 在可靠且有价值时返回 Transition/State。

如果 Edge 在 Face 上存在区间重合，必须表达 Overlap，而不是人为离散成很多采样点。

## Curve/Surface 后续

对于没有拓扑实体的 Sketch/Constraint Geometry，可另提供更底层的 Curve/Surface Intersection。它与 Edge/Face 分开，让调用者明确选择“Topology-bounded”还是“Pure Geometry”语义。

## 应用层边界

Bridge 只返回几何事实，不决定某个 Intersection 是 Snap Point、Constraint、Grip、Trim Candidate 或 Routing Waypoint。这些继续属于 CAD 应用层。

## 发布前测试

至少覆盖：

- 两直线单点相交；
- Endpoint Touch；
- 平行不相交；
- 共线部分重合；
- 完全重合的相同 Edge；
- Line / Arc；
- Tangent Contact；
- 支持多交点的 Curve Pair；
- Reversed Edge Orientation；
- Transform 后 Shape；
- Near Contact 在 Tolerance 上下边界；
- Bulk Result 顺序与零结果；
- Managed 层 Cross-session Input 拒绝。

## 决策

首批实现结构化 Edge/Edge Intersection：Point/Overlap 语义完整，从第一版直接 Bulk Transfer，并放入独立 Native Module。不要增加 Boolean-only Intersection，也不要重新引入 N+1 Indexed Result Retrieval。
