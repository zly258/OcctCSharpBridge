# OcctModelSurfaceDifferential

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelSurfaceDifferential
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelSurfaceDifferential`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelSurfaceDifferential(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d UDerivative, OcctVector3d VDerivative, OcctVector3d USecondDerivative, OcctVector3d VSecondDerivative, OcctVector3d UvDerivative, bool HasNormal)
```

**Parameters**

- `U` — `double`
- `V` — `double`
- `Point` — `OcctPoint3d`
- `Normal` — `OcctVector3d`
- `UDerivative` — `OcctVector3d`
- `VDerivative` — `OcctVector3d`
- `USecondDerivative` — `OcctVector3d`
- `VSecondDerivative` — `OcctVector3d`
- `UvDerivative` — `OcctVector3d`
- `HasNormal` — `bool`

## Properties

### `HasNormal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasNormal { get; set; }
```

### `Normal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Normal { get; set; }
```

### `Point`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Point { get; set; }
```

### `U`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double U { get; set; }
```

### `UDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d UDerivative { get; set; }
```

### `USecondDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d USecondDerivative { get; set; }
```

### `UvDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d UvDerivative { get; set; }
```

### `V`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double V { get; set; }
```

### `VDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d VDerivative { get; set; }
```

### `VSecondDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d VSecondDerivative { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double U, out double V, out OcctPoint3d Point, out OcctVector3d Normal, out OcctVector3d UDerivative, out OcctVector3d VDerivative, out OcctVector3d USecondDerivative, out OcctVector3d VSecondDerivative, out OcctVector3d UvDerivative, out bool HasNormal)
```

**Parameters**

- `U` — `out double`
- `V` — `out double`
- `Point` — `out OcctPoint3d`
- `Normal` — `out OcctVector3d`
- `UDerivative` — `out OcctVector3d`
- `VDerivative` — `out OcctVector3d`
- `USecondDerivative` — `out OcctVector3d`
- `VSecondDerivative` — `out OcctVector3d`
- `UvDerivative` — `out OcctVector3d`
- `HasNormal` — `out bool`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelSurfaceDifferential other)
```

**Parameters**

- `other` — `OcctModelSurfaceDifferential`

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

