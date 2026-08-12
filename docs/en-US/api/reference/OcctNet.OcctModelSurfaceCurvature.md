# OcctModelSurfaceCurvature

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelSurfaceCurvature
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelSurfaceCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelSurfaceCurvature(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d MaximumDirection, OcctVector3d MinimumDirection, double MaximumCurvature, double MinimumCurvature, double MeanCurvature, double GaussianCurvature, bool IsUmbilic, bool HasNormal, bool HasCurvature)
```

**Parameters**

- `U` — `double`
- `V` — `double`
- `Point` — `OcctPoint3d`
- `Normal` — `OcctVector3d`
- `MaximumDirection` — `OcctVector3d`
- `MinimumDirection` — `OcctVector3d`
- `MaximumCurvature` — `double`
- `MinimumCurvature` — `double`
- `MeanCurvature` — `double`
- `GaussianCurvature` — `double`
- `IsUmbilic` — `bool`
- `HasNormal` — `bool`
- `HasCurvature` — `bool`

## Properties

### `GaussianCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double GaussianCurvature { get; set; }
```

### `HasCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasCurvature { get; set; }
```

### `HasNormal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasNormal { get; set; }
```

### `IsUmbilic`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsUmbilic { get; set; }
```

### `MaximumCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double MaximumCurvature { get; set; }
```

### `MaximumDirection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d MaximumDirection { get; set; }
```

### `MeanCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double MeanCurvature { get; set; }
```

### `MinimumCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double MinimumCurvature { get; set; }
```

### `MinimumDirection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d MinimumDirection { get; set; }
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

### `V`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double V { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double U, out double V, out OcctPoint3d Point, out OcctVector3d Normal, out OcctVector3d MaximumDirection, out OcctVector3d MinimumDirection, out double MaximumCurvature, out double MinimumCurvature, out double MeanCurvature, out double GaussianCurvature, out bool IsUmbilic, out bool HasNormal, out bool HasCurvature)
```

**Parameters**

- `U` — `out double`
- `V` — `out double`
- `Point` — `out OcctPoint3d`
- `Normal` — `out OcctVector3d`
- `MaximumDirection` — `out OcctVector3d`
- `MinimumDirection` — `out OcctVector3d`
- `MaximumCurvature` — `out double`
- `MinimumCurvature` — `out double`
- `MeanCurvature` — `out double`
- `GaussianCurvature` — `out double`
- `IsUmbilic` — `out bool`
- `HasNormal` — `out bool`
- `HasCurvature` — `out bool`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelSurfaceCurvature other)
```

**Parameters**

- `other` — `OcctModelSurfaceCurvature`

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

