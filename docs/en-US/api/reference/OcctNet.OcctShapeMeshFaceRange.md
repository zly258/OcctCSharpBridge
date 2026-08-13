# OcctShapeMeshFaceRange

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Describes the contiguous node and triangle ranges contributed by one source face to a combined shape mesh.

## Constructors

None.

## Properties

### `Face`

```csharp
public OcctModelShape Face { get; }
```

### `NodeCount`

```csharp
public int NodeCount { get; }
```

### `NodeEndExclusive`

```csharp
public int NodeEndExclusive { get; }
```

### `NodeStart`

```csharp
public int NodeStart { get; }
```

### `TriangleCount`

```csharp
public int TriangleCount { get; }
```

### `TriangleEndExclusive`

```csharp
public int TriangleEndExclusive { get; }
```

### `TriangleStart`

```csharp
public int TriangleStart { get; }
```

## Events

None.

## Methods

### `ContainsNode`

```csharp
public bool ContainsNode(int nodeIndex)
```

### `ContainsTriangle`

```csharp
public bool ContainsTriangle(int triangleIndex)
```

## Fields / Enum Values

None.

