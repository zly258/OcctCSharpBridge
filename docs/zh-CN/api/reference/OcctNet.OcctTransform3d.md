# OcctTransform3d

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctTransform3d
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctTransform3d`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTransform3d(double M00, double M01, double M02, double M03, double M10, double M11, double M12, double M13, double M20, double M21, double M22, double M23)
```

**参数**

- `M00` — `double`
- `M01` — `double`
- `M02` — `double`
- `M03` — `double`
- `M10` — `double`
- `M11` — `double`
- `M12` — `double`
- `M13` — `double`
- `M20` — `double`
- `M21` — `double`
- `M22` — `double`
- `M23` — `double`

## 属性

### `Identity`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTransform3d Identity { get; }
```

### `IsFinite`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsFinite { get; }
```

### `M00`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M00 { get; set; }
```

### `M01`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M01 { get; set; }
```

### `M02`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M02 { get; set; }
```

### `M03`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M03 { get; set; }
```

### `M10`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M10 { get; set; }
```

### `M11`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M11 { get; set; }
```

### `M12`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M12 { get; set; }
```

### `M13`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M13 { get; set; }
```

### `M20`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M20 { get; set; }
```

### `M21`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M21 { get; set; }
```

### `M22`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M22 { get; set; }
```

### `M23`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double M23 { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double M00, out double M01, out double M02, out double M03, out double M10, out double M11, out double M12, out double M13, out double M20, out double M21, out double M22, out double M23)
```

**参数**

- `M00` — `out double`
- `M01` — `out double`
- `M02` — `out double`
- `M03` — `out double`
- `M10` — `out double`
- `M11` — `out double`
- `M12` — `out double`
- `M13` — `out double`
- `M20` — `out double`
- `M21` — `out double`
- `M22` — `out double`
- `M23` — `out double`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctTransform3d other)
```

**参数**

- `other` — `OcctTransform3d`

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

### `Translation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctTransform3d Translation(double x, double y, double z)
```

**参数**

- `x` — `double`
- `y` — `double`
- `z` — `double`

**返回值:** `OcctTransform3d`

## 字段 / 枚举值

无

