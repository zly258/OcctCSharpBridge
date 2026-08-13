# OcctSceneLightingSettings

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctSceneLightingSettings`

```csharp
public OcctSceneLightingSettings(Color AmbientColor, double AmbientIntensity, OcctDirectionalLightSettings CameraLight, OcctDirectionalLightSettings SunLight, OcctDirectionalLightSettings FillLight)
```

## Properties

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

## Events

None.

## Methods

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

## Fields / Enum Values

None.

