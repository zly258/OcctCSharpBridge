# OcctShapeMeshData

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Combined shape triangulation plus stable ranges that map mesh nodes and triangles back to the source OCCT faces that contributed them.

## Constructors

None.

## Properties

### `FaceCount`

```csharp
public int FaceCount { get; }
```

### `FaceRanges`

```csharp
public IReadOnlyList<OcctShapeMeshFaceRange> FaceRanges { get; }
```

### `Mesh`

```csharp
public OcctMesh Mesh { get; }
```

### `NodeCount`

```csharp
public int NodeCount { get; }
```

### `TriangleCount`

```csharp
public int TriangleCount { get; }
```

## Events

None.

## Methods

### `GetFaceForNode`

```csharp
public OcctModelShape GetFaceForNode(int nodeIndex)
```

### `GetFaceForTriangle`

```csharp
public OcctModelShape GetFaceForTriangle(int triangleIndex)
```

### `GetFaceRange`

```csharp
public OcctShapeMeshFaceRange GetFaceRange(int faceIndex)
```

### `TryGetFaceForNode`

```csharp
public bool TryGetFaceForNode(int nodeIndex, OcctModelShape face)
```

### `TryGetFaceForTriangle`

```csharp
public bool TryGetFaceForTriangle(int triangleIndex, OcctModelShape face)
```

## Fields / Enum Values

None.

