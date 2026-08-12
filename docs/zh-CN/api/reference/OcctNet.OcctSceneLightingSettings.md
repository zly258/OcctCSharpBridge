# OcctSceneLightingSettings

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctSceneLightingSettings
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctSceneLightingSettings`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctSceneLightingSettings(Color AmbientColor, double AmbientIntensity, OcctDirectionalLightSettings CameraLight, OcctDirectionalLightSettings SunLight, OcctDirectionalLightSettings FillLight)
```

**参数**

- `AmbientColor` — `Color`
- `AmbientIntensity` — `double`
- `CameraLight` — `OcctDirectionalLightSettings`
- `SunLight` — `OcctDirectionalLightSettings`
- `FillLight` — `OcctDirectionalLightSettings`

## 属性

### `AmbientColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Color AmbientColor { get; set; }
```

### `AmbientIntensity`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double AmbientIntensity { get; set; }
```

### `CameraLight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDirectionalLightSettings CameraLight { get; set; }
```

### `FillLight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDirectionalLightSettings FillLight { get; set; }
```

### `SunLight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDirectionalLightSettings SunLight { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out Color AmbientColor, out double AmbientIntensity, out OcctDirectionalLightSettings CameraLight, out OcctDirectionalLightSettings SunLight, out OcctDirectionalLightSettings FillLight)
```

**参数**

- `AmbientColor` — `out Color`
- `AmbientIntensity` — `out double`
- `CameraLight` — `out OcctDirectionalLightSettings`
- `SunLight` — `out OcctDirectionalLightSettings`
- `FillLight` — `out OcctDirectionalLightSettings`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctSceneLightingSettings other)
```

**参数**

- `other` — `OcctSceneLightingSettings`

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

