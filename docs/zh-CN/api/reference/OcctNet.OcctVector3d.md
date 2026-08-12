# OcctVector3d

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctVector3d
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctVector3d`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d(double x, double y, double z)
```

**参数**

- `x` — `double`
- `y` — `double`
- `z` — `double`

## 属性

### `IsFinite`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsFinite { get; }
```

### `Length`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Length { get; }
```

### `LengthSquared`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double LengthSquared { get; }
```

### `UnitX`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d UnitX { get; }
```

### `UnitY`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d UnitY { get; }
```

### `UnitZ`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d UnitZ { get; }
```

### `Zero`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Zero { get; }
```

## 事件

无

## 方法

### `Cross`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Cross(OcctVector3d other)
```

**参数**

- `other` — `OcctVector3d`

**返回值:** `OcctVector3d`

### `Dot`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Dot(OcctVector3d other)
```

**参数**

- `other` — `OcctVector3d`

**返回值:** `double`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctVector3d other)
```

**参数**

- `other` — `OcctVector3d`

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

### `Normalized`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Normalized()
```

**返回值:** `OcctVector3d`

### `ToString`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual string ToString()
```

**返回值:** `string`

### `TryNormalize`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryNormalize(out OcctVector3d result)
```

**参数**

- `result` — `out OcctVector3d`

**返回值:** `bool`

## 字段 / 枚举值

- `X` — `double` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `Y` — `double` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `Z` — `double` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

