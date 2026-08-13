# OcctInertiaProperties

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Linear, surface, or volume inertia properties. The inertia tensor and principal properties are expressed about the center of mass. Principal axes are expressed in the absolute coordinate system and may be non-unique for symmetric geometry.

## Constructors

### `OcctInertiaProperties`

Linear, surface, or volume inertia properties. The inertia tensor and principal properties are expressed about the center of mass. Principal axes are expressed in the absolute coordinate system and may be non-unique for symmetric geometry.

```csharp
public OcctInertiaProperties(double Mass, OcctPoint3d CenterOfMass, double Ixx, double Iyy, double Izz, double Ixy, double Ixz, double Iyz, double PrincipalMoment1, double PrincipalMoment2, double PrincipalMoment3, OcctVector3d PrincipalAxis1, OcctVector3d PrincipalAxis2, OcctVector3d PrincipalAxis3, double RadiusOfGyration1, double RadiusOfGyration2, double RadiusOfGyration3, bool HasSymmetryAxis, bool HasSymmetryPoint)
```

## Properties

### `CenterOfMass`

```csharp
public OcctPoint3d CenterOfMass { get; set; }
```

### `HasSymmetryAxis`

```csharp
public bool HasSymmetryAxis { get; set; }
```

### `HasSymmetryPoint`

```csharp
public bool HasSymmetryPoint { get; set; }
```

### `Ixx`

```csharp
public double Ixx { get; set; }
```

### `Ixy`

```csharp
public double Ixy { get; set; }
```

### `Ixz`

```csharp
public double Ixz { get; set; }
```

### `Iyy`

```csharp
public double Iyy { get; set; }
```

### `Iyz`

```csharp
public double Iyz { get; set; }
```

### `Izz`

```csharp
public double Izz { get; set; }
```

### `Mass`

```csharp
public double Mass { get; set; }
```

### `PrincipalAxis1`

```csharp
public OcctVector3d PrincipalAxis1 { get; set; }
```

### `PrincipalAxis2`

```csharp
public OcctVector3d PrincipalAxis2 { get; set; }
```

### `PrincipalAxis3`

```csharp
public OcctVector3d PrincipalAxis3 { get; set; }
```

### `PrincipalMoment1`

```csharp
public double PrincipalMoment1 { get; set; }
```

### `PrincipalMoment2`

```csharp
public double PrincipalMoment2 { get; set; }
```

### `PrincipalMoment3`

```csharp
public double PrincipalMoment3 { get; set; }
```

### `RadiusOfGyration1`

```csharp
public double RadiusOfGyration1 { get; set; }
```

### `RadiusOfGyration2`

```csharp
public double RadiusOfGyration2 { get; set; }
```

### `RadiusOfGyration3`

```csharp
public double RadiusOfGyration3 { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(double Mass, OcctPoint3d CenterOfMass, double Ixx, double Iyy, double Izz, double Ixy, double Ixz, double Iyz, double PrincipalMoment1, double PrincipalMoment2, double PrincipalMoment3, OcctVector3d PrincipalAxis1, OcctVector3d PrincipalAxis2, OcctVector3d PrincipalAxis3, double RadiusOfGyration1, double RadiusOfGyration2, double RadiusOfGyration3, bool HasSymmetryAxis, bool HasSymmetryPoint)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctInertiaProperties other)
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

