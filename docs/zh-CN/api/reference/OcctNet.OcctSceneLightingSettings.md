# OcctSceneLightingSettings

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctSceneLightingSettings`

```csharp
public OcctSceneLightingSettings(Color AmbientColor, double AmbientIntensity, OcctDirectionalLightSettings CameraLight, OcctDirectionalLightSettings SunLight, OcctDirectionalLightSettings FillLight)
```

## 属性

### `AmbientColor`

```csharp
public Color AmbientColor { get; set; }
```

### `AmbientIntensity`

```csharp
public double AmbientIntensity { get; set; }
```

### `CameraLight`

```csharp
public OcctDirectionalLightSettings CameraLight { get; set; }
```

### `FillLight`

```csharp
public OcctDirectionalLightSettings FillLight { get; set; }
```

### `SunLight`

```csharp
public OcctDirectionalLightSettings SunLight { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(Color AmbientColor, double AmbientIntensity, OcctDirectionalLightSettings CameraLight, OcctDirectionalLightSettings SunLight, OcctDirectionalLightSettings FillLight)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctSceneLightingSettings other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## 字段 / 枚举值

无。

