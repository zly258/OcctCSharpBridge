# OcctPoint3d

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctPoint3d
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctPoint3d`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d(double x, double y, double z)
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

### `Origin`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Origin { get; }
```

## 事件

无

## 方法

### `DistanceTo`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double DistanceTo(OcctPoint3d other)
```

**参数**

- `other` — `OcctPoint3d`

**返回值:** `double`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctPoint3d other)
```

**参数**

- `other` — `OcctPoint3d`

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

- `X` — `double` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `Y` — `double` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `Z` — `double` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

