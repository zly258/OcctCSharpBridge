# OcctModelParameterRange

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctModelParameterRange
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctModelParameterRange`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelParameterRange(double FirstParameter, double LastParameter, bool IsClosed, bool IsPeriodic, double Period)
```

**参数**

- `FirstParameter` — `double`
- `LastParameter` — `double`
- `IsClosed` — `bool`
- `IsPeriodic` — `bool`
- `Period` — `double`

## 属性

### `FirstParameter`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double FirstParameter { get; set; }
```

### `IsClosed`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsClosed { get; set; }
```

### `IsPeriodic`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsPeriodic { get; set; }
```

### `LastParameter`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double LastParameter { get; set; }
```

### `Length`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Length { get; }
```

### `Period`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Period { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out double FirstParameter, out double LastParameter, out bool IsClosed, out bool IsPeriodic, out double Period)
```

**参数**

- `FirstParameter` — `out double`
- `LastParameter` — `out double`
- `IsClosed` — `out bool`
- `IsPeriodic` — `out bool`
- `Period` — `out double`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctModelParameterRange other)
```

**参数**

- `other` — `OcctModelParameterRange`

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

