# OcctInertiaProperties

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctInertiaProperties
```

## 说明

Linear, surface, or volume inertia properties. The inertia tensor and principal properties are expressed about the center of mass. Principal axes are expressed in the absolute coordinate system and may be non-unique for symmetric geometry.

## 构造函数

### `OcctInertiaProperties`

Linear, surface, or volume inertia properties. The inertia tensor and principal properties are expressed about the center of mass. Principal axes are expressed in the absolute coordinate system and may be non-unique for symmetric geometry.

```csharp
public OcctInertiaProperties(double Mass, OcctPoint3d CenterOfMass, double Ixx, double Iyy, double Izz, double Ixy, double Ixz, double Iyz, double PrincipalMoment1, double PrincipalMoment2, double PrincipalMoment3, OcctVector3d PrincipalAxis1, OcctVector3d PrincipalAxis2, OcctVector3d PrincipalAxis3, double RadiusOfGyration1, double RadiusOfGyration2, double RadiusOfGyration3, bool HasSymmetryAxis, bool HasSymmetryPoint)
```

**参数**

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

## 属性

### `CenterOfMass`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d CenterOfMass { get; set; }
```

### `HasSymmetryAxis`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasSymmetryAxis { get; set; }
```

### `HasSymmetryPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasSymmetryPoint { get; set; }
```

### `Ixx`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Ixx { get; set; }
```

### `Ixy`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Ixy { get; set; }
```

### `Ixz`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Ixz { get; set; }
```

### `Iyy`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Iyy { get; set; }
```

### `Iyz`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Iyz { get; set; }
```

### `Izz`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Izz { get; set; }
```

### `Mass`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Mass { get; set; }
```

### `PrincipalAxis1`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d PrincipalAxis1 { get; set; }
```

### `PrincipalAxis2`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d PrincipalAxis2 { get; set; }
```

### `PrincipalAxis3`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d PrincipalAxis3 { get; set; }
```

### `PrincipalMoment1`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double PrincipalMoment1 { get; set; }
```

### `PrincipalMoment2`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double PrincipalMoment2 { get; set; }
```

### `PrincipalMoment3`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double PrincipalMoment3 { get; set; }
```

### `RadiusOfGyration1`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RadiusOfGyration1 { get; set; }
```

### `RadiusOfGyration2`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RadiusOfGyration2 { get; set; }
```

### `RadiusOfGyration3`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RadiusOfGyration3 { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double Mass, out OcctPoint3d CenterOfMass, out double Ixx, out double Iyy, out double Izz, out double Ixy, out double Ixz, out double Iyz, out double PrincipalMoment1, out double PrincipalMoment2, out double PrincipalMoment3, out OcctVector3d PrincipalAxis1, out OcctVector3d PrincipalAxis2, out OcctVector3d PrincipalAxis3, out double RadiusOfGyration1, out double RadiusOfGyration2, out double RadiusOfGyration3, out bool HasSymmetryAxis, out bool HasSymmetryPoint)
```

**参数**

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

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctInertiaProperties other)
```

**参数**

- `other` — `OcctInertiaProperties`

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

