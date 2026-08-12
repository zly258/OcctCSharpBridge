# OcctShapeMeshData

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctShapeMeshData
```

## Description

Combined shape triangulation plus stable ranges that map mesh nodes and triangles back to the source OCCT faces that contributed them.

## Constructors

None

## Properties

### `FaceCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int FaceCount { get; }
```

### `FaceRanges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctShapeMeshFaceRange> FaceRanges { get; }
```

### `Mesh`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMesh Mesh { get; }
```

### `NodeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int NodeCount { get; }
```

### `TriangleCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int TriangleCount { get; }
```

## Events

None

## Methods

### `GetFaceForNode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape GetFaceForNode(int nodeIndex)
```

**Parameters**

- `nodeIndex` — `int`

**Returns:** `OcctModelShape`

### `GetFaceForTriangle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape GetFaceForTriangle(int triangleIndex)
```

**Parameters**

- `triangleIndex` — `int`

**Returns:** `OcctModelShape`

### `GetFaceRange`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShapeMeshFaceRange GetFaceRange(int faceIndex)
```

**Parameters**

- `faceIndex` — `int`

**Returns:** `OcctShapeMeshFaceRange`

### `TryGetFaceForNode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetFaceForNode(int nodeIndex, out OcctModelShape face)
```

**Parameters**

- `nodeIndex` — `int`
- `face` — `out OcctModelShape`

**Returns:** `bool`

### `TryGetFaceForTriangle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetFaceForTriangle(int triangleIndex, out OcctModelShape face)
```

**Parameters**

- `triangleIndex` — `int`
- `face` — `out OcctModelShape`

**Returns:** `bool`

## Fields / Enum Values

None

