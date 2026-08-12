# OcctInertiaProperties

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctInertiaProperties
```

## Description

Linear, surface, or volume inertia properties. The inertia tensor and principal properties are expressed about the center of mass. Principal axes are expressed in the absolute coordinate system and may be non-unique for symmetric geometry.

## Constructors

### `OcctInertiaProperties`

Linear, surface, or volume inertia properties. The inertia tensor and principal properties are expressed about the center of mass. Principal axes are expressed in the absolute coordinate system and may be non-unique for symmetric geometry.

```csharp
public OcctInertiaProperties(double Mass, OcctPoint3d CenterOfMass, double Ixx, double Iyy, double Izz, double Ixy, double Ixz, double Iyz, double PrincipalMoment1, double PrincipalMoment2, double PrincipalMoment3, OcctVector3d PrincipalAxis1, OcctVector3d PrincipalAxis2, OcctVector3d PrincipalAxis3, double RadiusOfGyration1, double RadiusOfGyration2, double RadiusOfGyration3, bool HasSymmetryAxis, bool HasSymmetryPoint)
```

**Parameters**

- `Mass` — `double`
- `CenterOfMass` — `OcctPoint3d`
- `Ixx` — `double`
- `Iyy` — `double`
- `Izz` — `double`
- `Ixy` — `double`
- `Ixz` — `double`
- `Iyz` — `double`
- `PrincipalMoment1` — `double`
- `PrincipalMoment2` — `double`
- `PrincipalMoment3` — `double`
- `PrincipalAxis1` — `OcctVector3d`
- `PrincipalAxis2` — `OcctVector3d`
- `PrincipalAxis3` — `OcctVector3d`
- `RadiusOfGyration1` — `double`
- `RadiusOfGyration2` — `double`
- `RadiusOfGyration3` — `double`
- `HasSymmetryAxis` — `bool`
- `HasSymmetryPoint` — `bool`

## Properties

### `CenterOfMass`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d CenterOfMass { get; set; }
```

### `HasSymmetryAxis`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasSymmetryAxis { get; set; }
```

### `HasSymmetryPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasSymmetryPoint { get; set; }
```

### `Ixx`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Ixx { get; set; }
```

### `Ixy`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Ixy { get; set; }
```

### `Ixz`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Ixz { get; set; }
```

### `Iyy`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Iyy { get; set; }
```

### `Iyz`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Iyz { get; set; }
```

### `Izz`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Izz { get; set; }
```

### `Mass`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Mass { get; set; }
```

### `PrincipalAxis1`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d PrincipalAxis1 { get; set; }
```

### `PrincipalAxis2`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d PrincipalAxis2 { get; set; }
```

### `PrincipalAxis3`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d PrincipalAxis3 { get; set; }
```

### `PrincipalMoment1`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double PrincipalMoment1 { get; set; }
```

### `PrincipalMoment2`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double PrincipalMoment2 { get; set; }
```

### `PrincipalMoment3`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double PrincipalMoment3 { get; set; }
```

### `RadiusOfGyration1`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RadiusOfGyration1 { get; set; }
```

### `RadiusOfGyration2`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RadiusOfGyration2 { get; set; }
```

### `RadiusOfGyration3`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RadiusOfGyration3 { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double Mass, out OcctPoint3d CenterOfMass, out double Ixx, out double Iyy, out double Izz, out double Ixy, out double Ixz, out double Iyz, out double PrincipalMoment1, out double PrincipalMoment2, out double PrincipalMoment3, out OcctVector3d PrincipalAxis1, out OcctVector3d PrincipalAxis2, out OcctVector3d PrincipalAxis3, out double RadiusOfGyration1, out double RadiusOfGyration2, out double RadiusOfGyration3, out bool HasSymmetryAxis, out bool HasSymmetryPoint)
```

**Parameters**

- `Mass` — `out double`
- `CenterOfMass` — `out OcctPoint3d`
- `Ixx` — `out double`
- `Iyy` — `out double`
- `Izz` — `out double`
- `Ixy` — `out double`
- `Ixz` — `out double`
- `Iyz` — `out double`
- `PrincipalMoment1` — `out double`
- `PrincipalMoment2` — `out double`
- `PrincipalMoment3` — `out double`
- `PrincipalAxis1` — `out OcctVector3d`
- `PrincipalAxis2` — `out OcctVector3d`
- `PrincipalAxis3` — `out OcctVector3d`
- `RadiusOfGyration1` — `out double`
- `RadiusOfGyration2` — `out double`
- `RadiusOfGyration3` — `out double`
- `HasSymmetryAxis` — `out bool`
- `HasSymmetryPoint` — `out bool`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctInertiaProperties other)
```

**Parameters**

- `other` — `OcctInertiaProperties`

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

