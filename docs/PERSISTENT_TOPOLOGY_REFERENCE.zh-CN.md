# 持久拓扑引用设计

## 状态

本文定义未来可增量加入 Bridge 的设计契约。它**不引入 OCAF/XDE**，不改变 Native ABI 3，也不把 OCCT 子拓扑遍历索引宣称为“永久命名”。

## 问题

`OcctSelectionHit.SubshapeIndex` 和 `OcctModelingSession.GetSubshapeAt(...)` 当前明确表示 `TopExp_Explorer` 的运行时顺序。这个顺序适合一次拓扑状态内的交互，但 Boolean、Fillet、Healing、导入导出或特征重生成后，Face/Edge/Vertex 的索引可能改变。

因此 CAD 应用不能把 Runtime ShapeId、Hash 或 SubshapeIndex 直接持久化后当成稳定身份。

## 边界

Bridge 只提供**几何/拓扑客观事实、历史映射和解析证据**。Document、Feature/Entity 身份、持久化策略、用户确认和业务兜底由上层应用负责。

Bridge 中不写入应用层 `DocumentId`、`FeatureId`、Command History 或 JSON Document Schema。

## 非目标

Bridge 的拓扑引用不是：

- 保证永不变化的 Subshape ID；
- OCAF/XDE Label；
- 把 `TopExp_Explorer` 索引永久保存为真值；
- 跨 Session 保存的 `OcctObjectId` / `OcctModelShape.Id`；
- 假设单个几何 Hash 全局唯一；
- 在结果有歧义时仍强行返回某一个 Shape。

## 建议的 Reference

未来 `OcctTopologyReference` 应当是**有版本、可序列化、与应用文档解耦**的值对象，只保存中性的几何/拓扑证据。

建议字段：

| 字段 | 作用 |
|---|---|
| `Version` | 允许后续升级 Fingerprint/Resolver 算法 |
| `ShapeType` | Vertex / Edge / Face |
| `RuntimeIndexHint` | 只作快速提示，绝不是身份 |
| `GeometryType` | 可获得时记录解析 Curve/Surface 类型 |
| `Measure` | Edge Length 或 Face Area |
| `Center` | 几何/质量中心的空间特征 |
| `Bounds` | AABB 或归一化包围盒特征 |
| `Tolerance` | 原始拓扑容差 |
| `Orientation` | 辅助证据，不能独立作为身份 |
| `GeometryParameters` | Radius、Axis、Normal 等解析几何参数 |
| `AdjacencySignature` | 邻接拓扑的数量/类型特征 |
| `ParentSignature` | 可选的 Root/Owner Shape 证据 |

浮点量必须按明确的容差策略归一化后再比较或 Hash，不能使用二进制精确相等作为匹配依据。

## Resolver 流程

Resolver 应返回候选和证据，而不是过早强行选定一个 Subshape。

1. **Operation History**：如果调用方持有 `OcctOperationId`，优先检查 `Generated`、`Modified`、`IsRemoved`。这是局部特征重建后最强的证据。
2. **类型过滤**：不同拓扑类型直接排除。
3. **Runtime Index Hint**：先尝试旧索引以提高速度，但必须再次验证 Fingerprint。
4. **Geometry Filter**：比较解析 Curve/Surface 类型和不变量参数。
5. **Measure / Spatial Filter**：按容差比较 Length/Area、Center 和 Bounds。
6. **Adjacency Comparison**：比较相邻 Face/Edge/Vertex 特征，区分几何外形相似的候选。
7. **Score + Ambiguity**：对剩余候选评分；当第一、第二候选差距不足时明确返回 Ambiguous。

Resolver 不能为了“必须返回一个对象”而把歧义结果伪装成确定身份。

## 建议的解析状态

未来至少区分：

- `Resolved`：唯一候选明显满足匹配策略；
- `Ambiguous`：存在多个合理候选；
- `Removed`：Operation History 明确表示已删除；
- `NotFound`：没有候选达到最低得分；
- `InvalidReference`：版本不支持或 Reference 本身无效。

建议同时返回：

- resolved/candidate `OcctModelShape`；
- 评分；
- 各评分分量或 Match Flag；
- Candidate Count；
- 是否使用 Operation History；
- Runtime Index 是否命中；
- 第一、第二候选评分差值。

## 评分原则

Bridge 不应把某一种 CAD 业务规则硬编码进 Native。Bridge 可以提供保守的默认评分，并暴露客观匹配项，上层应用可使用更严格的业务阈值。

默认权重优先级建议：

1. Operation History 对应关系；
2. Topology Type + Analytic Geometry Type；
3. 解析几何不变量参数；
4. Adjacency Signature；
5. Measure；
6. Center / Bounds；
7. Runtime Index Hint。

Runtime Index 的语义权重最低。

## Operation History

Bridge 2.6 已提供 Generated / Modified / Removed 历史，并且 Generated/Modified 集合已经改为 Bulk Native ABI 获取。Persistent Reference 应复用这套历史，而不是额外再造一套互不关联的 History。

Operation History 只在当前 Operation/Session 范围内有效，本身仍不是跨文件 Persistent Naming。没有 History 时再退回几何/拓扑 Fingerprint。

## 对称与重复几何

对称模型和阵列模型天然可能存在歧义：两个 Subshape 可能有完全相同的几何与邻接关系。此时 API 必须暴露 `Ambiguous`，上层再利用 Feature Context、用户选择、装配上下文或自己的语义身份决定。

典型场景包括重复孔、等半径 Fillet、阵列 Face 和镜像特征。

## 持久化

Bridge Reference 应当容易序列化，但 Bridge 不拥有持久化格式。应用可以写 JSON、数据库、自定义 Document 或其它格式。

持久记录必须包含算法版本，这样未来可以升级 Fingerprint，而不是把第一版算法永久冻结成兼容负担。

## 实施阶段

### Phase 1：客观 Fingerprint

先在 Native/Internal 层生成：

- Type / Orientation / Tolerance；
- Analytic Geometry Type 与稳定参数；
- Measure / Center；
- Bounds；
- Local Adjacency Count / Type。

此阶段不发布 Public Resolver。

### Phase 2：内部 Candidate Resolver

新增 `OcctModelingTopologyReference.cpp`，实现候选生成和评分；用 Primitive、Boolean、Fillet、Chamfer、Same-Domain Unify、Healing 等场景验证。

### Phase 3：Additive Public API

只有在 Resolver 行为具备确定性测试后，才公开版本化 DTO 和解析方法。Runtime Index 仍只作为 Hint，并明确返回歧义。

### Phase 4：应用集成

`demo` 或其它 CAD 应用再把自己的 Feature/Entity ID 与中性的 Bridge Reference 关联，并按自身 Document 机制持久化。

## Public API 发布前必须覆盖的测试

至少包括：

- 拓扑未变化时准确解析；
- 单纯遍历顺序变化不导致引用失效；
- Boolean `Generated` / `Modified` 优先于 Fingerprint；
- 删除的拓扑返回 `Removed`；
- 等半径/重复特征可以返回 `Ambiguous`；
- 仅 Transform 变化不会错误破坏几何不变量；
- Tolerance 变化按策略处理；
- Import/Export Round Trip 不依赖 Runtime ID；
- 一个 Modeling Session 的 Reference 不能被误当成另一个 Session 的 Live Handle。

## 决策

Bridge 2.6 暂不发布简单的 `PersistentSubshapeId`。正确下一步是先实现**中性、版本化的 Topology Fingerprint + 歧义感知 Resolver**，Operation History 作为最强证据，应用层身份继续留在 Bridge 之上。
