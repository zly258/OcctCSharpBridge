# OcctFaceAnalysisInfo

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctFaceAnalysisInfo
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctFaceAnalysisInfo`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctFaceAnalysisInfo(OcctModelShape Face, OcctSurfaceType SurfaceType, OcctModelOrientation Orientation, double Area, double Tolerance, OcctUvBounds UvBounds, OcctBounds Bounds, int EdgeCount, int WireCount)
```

**Parameters**

- `Face` — `OcctModelShape`
- `SurfaceType` — `OcctSurfaceType`
- `Orientation` — `OcctModelOrientation`
- `Area` — `double`
- `Tolerance` — `double`
- `UvBounds` — `OcctUvBounds`
- `Bounds` — `OcctBounds`
- `EdgeCount` — `int`
- `WireCount` — `int`

## Properties

### `Area`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Area { get; set; }
```

### `Bounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctBounds Bounds { get; set; }
```

### `EdgeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int EdgeCount { get; set; }
```

### `Face`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Face { get; set; }
```

### `IsAnalytic`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsAnalytic { get; }
```

### `IsFreeform`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsFreeform { get; }
```

### `Orientation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelOrientation Orientation { get; set; }
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

### `UvBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctUvBounds UvBounds { get; set; }
```

### `WireCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int WireCount { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctModelShape Face, out OcctSurfaceType SurfaceType, out OcctModelOrientation Orientation, out double Area, out double Tolerance, out OcctUvBounds UvBounds, out OcctBounds Bounds, out int EdgeCount, out int WireCount)
```

**Parameters**

- `Face` — `out OcctModelShape`
- `SurfaceType` — `out OcctSurfaceType`
- `Orientation` — `out OcctModelOrientation`
- `Area` — `out double`
- `Tolerance` — `out double`
- `UvBounds` — `out OcctUvBounds`
- `Bounds` — `out OcctBounds`
- `EdgeCount` — `out int`
- `WireCount` — `out int`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctFaceAnalysisInfo other)
```

**Parameters**

- `other` — `OcctFaceAnalysisInfo`

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

