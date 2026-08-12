# OcctEdgeIntersection

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctEdgeIntersection
```

## 说明

A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values. For a point intersection, start and end points/parameters are equal.

## 构造函数

### `OcctEdgeIntersection`

A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values. For a point intersection, start and end points/parameters are equal.

```csharp
public OcctEdgeIntersection(OcctIntersectionKind Kind, OcctPoint3d StartPoint, OcctPoint3d EndPoint, double FirstParameterStart, double FirstParameterEnd, double SecondParameterStart, double SecondParameterEnd)
```

**参数**

- `Kind` — `OcctIntersectionKind`
- `StartPoint` — `OcctPoint3d`
- `EndPoint` — `OcctPoint3d`
- `FirstParameterStart` — `double`
- `FirstParameterEnd` — `double`
- `SecondParameterStart` — `double`
- `SecondParameterEnd` — `double`

## 属性

### `EndPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d EndPoint { get; set; }
```

### `FirstParameterEnd`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double FirstParameterEnd { get; set; }
```

### `FirstParameterStart`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double FirstParameterStart { get; set; }
```

### `Kind`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctIntersectionKind Kind { get; set; }
```

### `SecondParameterEnd`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double SecondParameterEnd { get; set; }
```

### `SecondParameterStart`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double SecondParameterStart { get; set; }
```

### `StartPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d StartPoint { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctIntersectionKind Kind, out OcctPoint3d StartPoint, out OcctPoint3d EndPoint, out double FirstParameterStart, out double FirstParameterEnd, out double SecondParameterStart, out double SecondParameterEnd)
```

**参数**

- `Kind` — `out OcctIntersectionKind`
- `StartPoint` — `out OcctPoint3d`
- `EndPoint` — `out OcctPoint3d`
- `FirstParameterStart` — `out double`
- `FirstParameterEnd` — `out double`
- `SecondParameterStart` — `out double`
- `SecondParameterEnd` — `out double`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctEdgeIntersection other)
```

**参数**

- `other` — `OcctEdgeIntersection`

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

