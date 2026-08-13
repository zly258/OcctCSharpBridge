# OcctTopologyReference

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape. RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.

## Constructors

### `OcctTopologyReference`

Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape. RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.

```csharp
public OcctTopologyReference(int Version, OcctShapeType ShapeType, int RuntimeIndexHint, OcctCurveType CurveType, OcctSurfaceType SurfaceType, double Measure, OcctPoint3d Center, OcctBounds Bounds, double Tolerance, OcctModelOrientation Orientation, int VertexCount, int EdgeCount, int FaceCount)
```

## Properties

### `Bounds`

```csharp
public OcctBounds Bounds { get; set; }
```

### `Center`

```csharp
public OcctPoint3d Center { get; set; }
```

### `CurveType`

```csharp
public OcctCurveType CurveType { get; set; }
```

### `EdgeCount`

```csharp
public int EdgeCount { get; set; }
```

### `FaceCount`

```csharp
public int FaceCount { get; set; }
```

### `Measure`

```csharp
public double Measure { get; set; }
```

### `Orientation`

```csharp
public OcctModelOrientation Orientation { get; set; }
```

### `RuntimeIndexHint`

```csharp
public int RuntimeIndexHint { get; set; }
```

### `ShapeType`

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `SurfaceType`

```csharp
public OcctSurfaceType SurfaceType { get; set; }
```

### `Tolerance`

```csharp
public double Tolerance { get; set; }
```

### `Version`

```csharp
public int Version { get; set; }
```

### `VertexCount`

```csharp
public int VertexCount { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(int Version, OcctShapeType ShapeType, int RuntimeIndexHint, OcctCurveType CurveType, OcctSurfaceType SurfaceType, double Measure, OcctPoint3d Center, OcctBounds Bounds, double Tolerance, OcctModelOrientation Orientation, int VertexCount, int EdgeCount, int FaceCount)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTopologyReference other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## Fields / Enum Values

None.

