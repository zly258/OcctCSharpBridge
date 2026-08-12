# OcctAssemblyStyle

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctAssemblyStyle
```

## 说明

Presentation style resolved by OCCT for one XDE assembly occurrence.

## 构造函数

### `OcctAssemblyStyle`

Presentation style resolved by OCCT for one XDE assembly occurrence.

```csharp
public OcctAssemblyStyle(bool Visible, OcctAssemblyColor? SurfaceColor, OcctAssemblyColor? CurveColor)
```

**参数**

- `Visible` — `bool`
- `SurfaceColor` — `OcctAssemblyColor?`
- `CurveColor` — `OcctAssemblyColor?`

## 属性

### `CurveColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctAssemblyColor? CurveColor { get; set; }
```

### `SurfaceColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctAssemblyColor? SurfaceColor { get; set; }
```

### `Transparency`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Transparency { get; }
```

### `Visible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Visible { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out bool Visible, out OcctAssemblyColor? SurfaceColor, out OcctAssemblyColor? CurveColor)
```

**参数**

- `Visible` — `out bool`
- `SurfaceColor` — `out OcctAssemblyColor?`
- `CurveColor` — `out OcctAssemblyColor?`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctAssemblyStyle other)
```

**参数**

- `other` — `OcctAssemblyStyle`

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

