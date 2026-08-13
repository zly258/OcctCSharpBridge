# OcctEdgeAdjacencyResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Immutable snapshot of all edge-to-face adjacency counts for one root shape. The native topology map is built once for the whole snapshot.

## Constructors

None.

## Properties

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

## Events

None.

## Methods

### `GetEdgesByAdjacentFaceCount`

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(int minimumFaceCount, int maximumFaceCount)
```

## Fields / Enum Values

None.

