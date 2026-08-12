# OcctTopologyReference

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctTopologyReference
```

## Description

Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape. RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.

## Constructors

### `OcctTopologyReference`

Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape. RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.

```csharp
public OcctTopologyReference(int Version, OcctShapeType ShapeType, int RuntimeIndexHint, OcctCurveType CurveType, OcctSurfaceType SurfaceType, double Measure, OcctPoint3d Center, OcctBounds Bounds, double Tolerance, OcctModelOrientation Orientation, int VertexCount, int EdgeCount, int FaceCount)
```

**Parameters**

- `Version` — `int`
- `ShapeType` — `OcctShapeType`
- `RuntimeIndexHint` — `int`
- `CurveType` — `OcctCurveType`
- `SurfaceType` — `OcctSurfaceType`
- `Measure` — `double`
- `Center` — `OcctPoint3d`
- `Bounds` — `OcctBounds`
- `Tolerance` — `double`
- `Orientation` — `OcctModelOrientation`
- `VertexCount` — `int`
- `EdgeCount` — `int`
- `FaceCount` — `int`

## Properties

### `Bounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctBounds Bounds { get; set; }
```

### `Center`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Center { get; set; }
```

### `CurveType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctCurveType CurveType { get; set; }
```

### `EdgeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int EdgeCount { get; set; }
```

### `FaceCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int FaceCount { get; set; }
```

### `Measure`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Measure { get; set; }
```

### `Orientation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelOrientation Orientation { get; set; }
```

### `RuntimeIndexHint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int RuntimeIndexHint { get; set; }
```

### `ShapeType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `SurfaceType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctSurfaceType SurfaceType { get; set; }
```

### `Tolerance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Tolerance { get; set; }
```

### `Version`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int Version { get; set; }
```

### `VertexCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int VertexCount { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out int Version, out OcctShapeType ShapeType, out int RuntimeIndexHint, out OcctCurveType CurveType, out OcctSurfaceType SurfaceType, out double Measure, out OcctPoint3d Center, out OcctBounds Bounds, out double Tolerance, out OcctModelOrientation Orientation, out int VertexCount, out int EdgeCount, out int FaceCount)
```

**Parameters**

- `Version` — `out int`
- `ShapeType` — `out OcctShapeType`
- `RuntimeIndexHint` — `out int`
- `CurveType` — `out OcctCurveType`
- `SurfaceType` — `out OcctSurfaceType`
- `Measure` — `out double`
- `Center` — `out OcctPoint3d`
- `Bounds` — `out OcctBounds`
- `Tolerance` — `out double`
- `Orientation` — `out OcctModelOrientation`
- `VertexCount` — `out int`
- `EdgeCount` — `out int`
- `FaceCount` — `out int`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctTopologyReference other)
```

**Parameters**

- `other` — `OcctTopologyReference`

**Returns:** `bool`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual bool Equals(object obj)
```

**Parameters**

- `obj` — `object`

**Returns:** `bool`

### `GetHashCode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual int GetHashCode()
```

**Returns:** `int`

### `ToString`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual string ToString()
```

**Returns:** `string`

## Fields / Enum Values

None

