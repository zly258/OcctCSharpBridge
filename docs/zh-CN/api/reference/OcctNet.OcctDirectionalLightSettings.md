# OcctDirectionalLightSettings

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctDirectionalLightSettings
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctDirectionalLightSettings`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDirectionalLightSettings(bool Enabled, Color Color, double Intensity, OcctVector3d Direction)
```

**参数**

- `Enabled` — `bool`
- `Color` — `Color`
- `Intensity` — `double`
- `Direction` — `OcctVector3d`

## 属性

### `Color`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Color Color { get; set; }
```

### `Direction`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Direction { get; set; }
```

### `Enabled`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Enabled { get; set; }
```

### `Intensity`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Intensity { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out bool Enabled, out Color Color, out double Intensity, out OcctVector3d Direction)
```

**参数**

- `Enabled` — `out bool`
- `Color` — `out Color`
- `Intensity` — `out double`
- `Direction` — `out OcctVector3d`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctDirectionalLightSettings other)
```

**参数**

- `other` — `OcctDirectionalLightSettings`

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

