# OcctModelCurveCurvature

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelCurveCurvature
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelCurveCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelCurveCurvature(double Parameter, OcctPoint3d Point, OcctVector3d Tangent, OcctVector3d Normal, OcctPoint3d CenterOfCurvature, double Curvature, bool HasTangent, bool HasNormal, bool HasCenterOfCurvature)
```

**Parameters**

- `Parameter` — `double`
- `Point` — `OcctPoint3d`
- `Tangent` — `OcctVector3d`
- `Normal` — `OcctVector3d`
- `CenterOfCurvature` — `OcctPoint3d`
- `Curvature` — `double`
- `HasTangent` — `bool`
- `HasNormal` — `bool`
- `HasCenterOfCurvature` — `bool`

## Properties

### `CenterOfCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d CenterOfCurvature { get; set; }
```

### `Curvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Curvature { get; set; }
```

### `HasCenterOfCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasCenterOfCurvature { get; set; }
```

### `HasNormal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasNormal { get; set; }
```

### `HasTangent`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasTangent { get; set; }
```

### `Normal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Normal { get; set; }
```

### `Parameter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Parameter { get; set; }
```

### `Point`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Point { get; set; }
```

### `RadiusOfCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RadiusOfCurvature { get; }
```

### `Tangent`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Tangent { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double Parameter, out OcctPoint3d Point, out OcctVector3d Tangent, out OcctVector3d Normal, out OcctPoint3d CenterOfCurvature, out double Curvature, out bool HasTangent, out bool HasNormal, out bool HasCenterOfCurvature)
```

**Parameters**

- `Parameter` — `out double`
- `Point` — `out OcctPoint3d`
- `Tangent` — `out OcctVector3d`
- `Normal` — `out OcctVector3d`
- `CenterOfCurvature` — `out OcctPoint3d`
- `Curvature` — `out double`
- `HasTangent` — `out bool`
- `HasNormal` — `out bool`
- `HasCenterOfCurvature` — `out bool`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelCurveCurvature other)
```

**Parameters**

- `other` — `OcctModelCurveCurvature`

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

