# OcctTopologyReference

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctTopologyReference
```

## 说明

Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape. RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.

## 构造函数

### `OcctTopologyReference`

Versioned geometric/topological fingerprint for a Vertex, Edge, or Face inside a root shape. RuntimeIndexHint is only a low-weight lookup hint and is never treated as persistent identity.

```csharp
public OcctTopologyReference(int Version, OcctShapeType ShapeType, int RuntimeIndexHint, OcctCurveType CurveType, OcctSurfaceType SurfaceType, double Measure, OcctPoint3d Center, OcctBounds Bounds, double Tolerance, OcctModelOrientation Orientation, int VertexCount, int EdgeCount, int FaceCount)
```

**参数**

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

## 属性

### `Bounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctBounds Bounds { get; set; }
```

### `Center`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Center { get; set; }
```

### `CurveType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctCurveType CurveType { get; set; }
```

### `EdgeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int EdgeCount { get; set; }
```

### `FaceCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int FaceCount { get; set; }
```

### `Measure`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Measure { get; set; }
```

### `Orientation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelOrientation Orientation { get; set; }
```

### `RuntimeIndexHint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int RuntimeIndexHint { get; set; }
```

### `ShapeType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `SurfaceType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctSurfaceType SurfaceType { get; set; }
```

### `Tolerance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Tolerance { get; set; }
```

### `Version`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int Version { get; set; }
```

### `VertexCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int VertexCount { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out int Version, out OcctShapeType ShapeType, out int RuntimeIndexHint, out OcctCurveType CurveType, out OcctSurfaceType SurfaceType, out double Measure, out OcctPoint3d Center, out OcctBounds Bounds, out double Tolerance, out OcctModelOrientation Orientation, out int VertexCount, out int EdgeCount, out int FaceCount)
```

**参数**

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

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctTopologyReference other)
```

**参数**

- `other` — `OcctTopologyReference`

**返回值:** `bool`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual bool Equals(object obj)
```

**参数**

- `obj` — `object`

**返回值:** `bool`

### `GetHashCode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual int GetHashCode()
```

**返回值:** `int`

### `ToString`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual string ToString()
```

**返回值:** `string`

## 字段 / 枚举值

无

