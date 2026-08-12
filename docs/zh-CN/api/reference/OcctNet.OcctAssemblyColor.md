# OcctAssemblyColor

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctAssemblyColor
```

## 说明

RGBA color retained from XDE presentation style.

## 构造函数

### `OcctAssemblyColor`

RGBA color retained from XDE presentation style.

```csharp
public OcctAssemblyColor(double R, double G, double B, double A = 1)
```

**参数**

- `R` — `double`
- `G` — `double`
- `B` — `double`
- `A` — `double` = 1

## 属性

### `A`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double A { get; set; }
```

### `B`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double B { get; set; }
```

### `G`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double G { get; set; }
```

### `R`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double R { get; set; }
```

### `Transparency`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Transparency { get; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double R, out double G, out double B, out double A)
```

**参数**

- `R` — `out double`
- `G` — `out double`
- `B` — `out double`
- `A` — `out double`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctAssemblyColor other)
```

**参数**

- `other` — `OcctAssemblyColor`

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

