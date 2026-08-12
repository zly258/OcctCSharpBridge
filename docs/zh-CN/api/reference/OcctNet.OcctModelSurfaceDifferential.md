# OcctModelSurfaceDifferential

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctModelSurfaceDifferential
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctModelSurfaceDifferential`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelSurfaceDifferential(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d UDerivative, OcctVector3d VDerivative, OcctVector3d USecondDerivative, OcctVector3d VSecondDerivative, OcctVector3d UvDerivative, bool HasNormal)
```

**参数**

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

## 属性

### `HasNormal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasNormal { get; set; }
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

### `UDerivative`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d UDerivative { get; set; }
```

### `USecondDerivative`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d USecondDerivative { get; set; }
```

### `UvDerivative`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d UvDerivative { get; set; }
```

### `V`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double V { get; set; }
```

### `VDerivative`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d VDerivative { get; set; }
```

### `VSecondDerivative`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d VSecondDerivative { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double U, out double V, out OcctPoint3d Point, out OcctVector3d Normal, out OcctVector3d UDerivative, out OcctVector3d VDerivative, out OcctVector3d USecondDerivative, out OcctVector3d VSecondDerivative, out OcctVector3d UvDerivative, out bool HasNormal)
```

**参数**

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

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctModelSurfaceDifferential other)
```

**参数**

- `other` — `OcctModelSurfaceDifferential`

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

