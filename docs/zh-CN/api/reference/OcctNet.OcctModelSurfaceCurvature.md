# OcctModelSurfaceCurvature

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctModelSurfaceCurvature
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctModelSurfaceCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelSurfaceCurvature(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d MaximumDirection, OcctVector3d MinimumDirection, double MaximumCurvature, double MinimumCurvature, double MeanCurvature, double GaussianCurvature, bool IsUmbilic, bool HasNormal, bool HasCurvature)
```

**参数**

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

## 属性

### `GaussianCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double GaussianCurvature { get; set; }
```

### `HasCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasCurvature { get; set; }
```

### `HasNormal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasNormal { get; set; }
```

### `IsUmbilic`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsUmbilic { get; set; }
```

### `MaximumCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double MaximumCurvature { get; set; }
```

### `MaximumDirection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d MaximumDirection { get; set; }
```

### `MeanCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double MeanCurvature { get; set; }
```

### `MinimumCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double MinimumCurvature { get; set; }
```

### `MinimumDirection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d MinimumDirection { get; set; }
```

### `Normal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Normal { get; set; }
```

### `Point`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Point { get; set; }
```

### `U`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double U { get; set; }
```

### `V`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double V { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double U, out double V, out OcctPoint3d Point, out OcctVector3d Normal, out OcctVector3d MaximumDirection, out OcctVector3d MinimumDirection, out double MaximumCurvature, out double MinimumCurvature, out double MeanCurvature, out double GaussianCurvature, out bool IsUmbilic, out bool HasNormal, out bool HasCurvature)
```

**参数**

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

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctModelSurfaceCurvature other)
```

**参数**

- `other` — `OcctModelSurfaceCurvature`

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

