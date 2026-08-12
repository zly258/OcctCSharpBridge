# OcctModelCurveCurvature

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctModelCurveCurvature
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctModelCurveCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelCurveCurvature(double Parameter, OcctPoint3d Point, OcctVector3d Tangent, OcctVector3d Normal, OcctPoint3d CenterOfCurvature, double Curvature, bool HasTangent, bool HasNormal, bool HasCenterOfCurvature)
```

**参数**

- `Parameter` — `double`
- `Point` — `OcctPoint3d`
- `Tangent` — `OcctVector3d`
- `Normal` — `OcctVector3d`
- `CenterOfCurvature` — `OcctPoint3d`
- `Curvature` — `double`
- `HasTangent` — `bool`
- `HasNormal` — `bool`
- `HasCenterOfCurvature` — `bool`

## 属性

### `CenterOfCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d CenterOfCurvature { get; set; }
```

### `Curvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Curvature { get; set; }
```

### `HasCenterOfCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasCenterOfCurvature { get; set; }
```

### `HasNormal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasNormal { get; set; }
```

### `HasTangent`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasTangent { get; set; }
```

### `Normal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Normal { get; set; }
```

### `Parameter`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Parameter { get; set; }
```

### `Point`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Point { get; set; }
```

### `RadiusOfCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RadiusOfCurvature { get; }
```

### `Tangent`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Tangent { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double Parameter, out OcctPoint3d Point, out OcctVector3d Tangent, out OcctVector3d Normal, out OcctPoint3d CenterOfCurvature, out double Curvature, out bool HasTangent, out bool HasNormal, out bool HasCenterOfCurvature)
```

**参数**

- `Parameter` — `out double`
- `Point` — `out OcctPoint3d`
- `Tangent` — `out OcctVector3d`
- `Normal` — `out OcctVector3d`
- `CenterOfCurvature` — `out OcctPoint3d`
- `Curvature` — `out double`
- `HasTangent` — `out bool`
- `HasNormal` — `out bool`
- `HasCenterOfCurvature` — `out bool`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctModelCurveCurvature other)
```

**参数**

- `other` — `OcctModelCurveCurvature`

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

