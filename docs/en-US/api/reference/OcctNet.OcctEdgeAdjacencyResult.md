# OcctEdgeAdjacencyResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctEdgeAdjacencyResult
```

## Description

Immutable snapshot of all edge-to-face adjacency counts for one root shape. The native topology map is built once for the whole snapshot.

## Constructors

None

## Properties

### `BoundaryCandidates`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> BoundaryCandidates { get; }
```

### `EdgeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int EdgeCount { get; }
```

### `Entries`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctEdgeAdjacencyInfo> Entries { get; }
```

### `HasBoundaryCandidates`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasBoundaryCandidates { get; }
```

### `HasNonManifoldEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasNonManifoldEdges { get; }
```

### `IsolatedEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> IsolatedEdges { get; }
```

### `ManifoldInteriorEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> ManifoldInteriorEdges { get; }
```

### `NonManifoldEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> NonManifoldEdges { get; }
```

### `Root`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Root { get; }
```

## Events

None

## Methods

### `GetEdgesByAdjacentFaceCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(int minimumFaceCount, int maximumFaceCount)
```

**Parameters**

- `minimumFaceCount` — `int`
- `maximumFaceCount` — `int`

**Returns:** `IReadOnlyList<OcctModelShape>`

## Fields / Enum Values

None

