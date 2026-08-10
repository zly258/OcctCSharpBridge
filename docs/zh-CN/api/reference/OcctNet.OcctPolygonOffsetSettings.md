# OcctPolygonOffsetSettings

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctPolygonOffsetSettings
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctPolygonOffsetSettings`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPolygonOffsetSettings(OcctPolygonOffsetMode Mode, double Factor, double Units)
```

**参数**

- `Mode` — `OcctPolygonOffsetMode`
- `Factor` — `double`
- `Units` — `double`

## 属性

### `Factor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Factor { get; set; }
```

### `Mode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPolygonOffsetMode Mode { get; set; }
```

### `Units`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Units { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctPolygonOffsetMode Mode, out double Factor, out double Units)
```

**参数**

- `Mode` — `out OcctPolygonOffsetMode`
- `Factor` — `out double`
- `Units` — `out double`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctPolygonOffsetSettings other)
```

**参数**

- `other` — `OcctPolygonOffsetSettings`

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

