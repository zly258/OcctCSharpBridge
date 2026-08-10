# OcctEdgeAdjacencyResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctEdgeAdjacencyResult
```

## 说明

Immutable snapshot of all edge-to-face adjacency counts for one root shape. The native topology map is built once for the whole snapshot.

## 构造函数

无

## 属性

### `BoundaryCandidates`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> BoundaryCandidates { get; }
```

### `EdgeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int EdgeCount { get; }
```

### `Entries`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctEdgeAdjacencyInfo> Entries { get; }
```

### `HasBoundaryCandidates`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasBoundaryCandidates { get; }
```

### `HasNonManifoldEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasNonManifoldEdges { get; }
```

### `IsolatedEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> IsolatedEdges { get; }
```

### `ManifoldInteriorEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> ManifoldInteriorEdges { get; }
```

### `NonManifoldEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> NonManifoldEdges { get; }
```

### `Root`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Root { get; }
```

## 事件

无

## 方法

### `GetEdgesByAdjacentFaceCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(int minimumFaceCount, int maximumFaceCount)
```

**参数**

- `minimumFaceCount` — `int`
- `maximumFaceCount` — `int`

**返回值:** `IReadOnlyList<OcctModelShape>`

## 字段 / 枚举值

无

