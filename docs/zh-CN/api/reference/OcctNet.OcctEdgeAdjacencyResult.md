# OcctEdgeAdjacencyResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Immutable snapshot of all edge-to-face adjacency counts for one root shape. The native topology map is built once for the whole snapshot.

## 构造函数

无。

## 属性

### `BoundaryCandidates`

```csharp
public IReadOnlyList<OcctModelShape> BoundaryCandidates { get; }
```

### `EdgeCount`

```csharp
public int EdgeCount { get; }
```

### `Entries`

```csharp
public IReadOnlyList<OcctEdgeAdjacencyInfo> Entries { get; }
```

### `HasBoundaryCandidates`

```csharp
public bool HasBoundaryCandidates { get; }
```

### `HasNonManifoldEdges`

```csharp
public bool HasNonManifoldEdges { get; }
```

### `IsolatedEdges`

```csharp
public IReadOnlyList<OcctModelShape> IsolatedEdges { get; }
```

### `ManifoldInteriorEdges`

```csharp
public IReadOnlyList<OcctModelShape> ManifoldInteriorEdges { get; }
```

### `NonManifoldEdges`

```csharp
public IReadOnlyList<OcctModelShape> NonManifoldEdges { get; }
```

### `Root`

```csharp
public OcctModelShape Root { get; }
```

## 事件

无。

## 方法

### `GetEdgesByAdjacentFaceCount`

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(int minimumFaceCount, int maximumFaceCount)
```

## 字段 / 枚举值

无。

