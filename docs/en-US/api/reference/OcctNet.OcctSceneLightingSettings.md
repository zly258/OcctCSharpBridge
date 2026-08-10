# OcctSceneLightingSettings

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctSceneLightingSettings
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctSceneLightingSettings`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctSceneLightingSettings(Color AmbientColor, double AmbientIntensity, OcctDirectionalLightSettings CameraLight, OcctDirectionalLightSettings SunLight, OcctDirectionalLightSettings FillLight)
```

**Parameters**

- `AmbientColor` — `Color`
- `AmbientIntensity` — `double`
- `CameraLight` — `OcctDirectionalLightSettings`
- `SunLight` — `OcctDirectionalLightSettings`
- `FillLight` — `OcctDirectionalLightSettings`

## Properties

### `AmbientColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public Color AmbientColor { get; set; }
```

### `AmbientIntensity`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double AmbientIntensity { get; set; }
```

### `CameraLight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDirectionalLightSettings CameraLight { get; set; }
```

### `FillLight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDirectionalLightSettings FillLight { get; set; }
```

### `SunLight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDirectionalLightSettings SunLight { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out Color AmbientColor, out double AmbientIntensity, out OcctDirectionalLightSettings CameraLight, out OcctDirectionalLightSettings SunLight, out OcctDirectionalLightSettings FillLight)
```

**Parameters**

- `AmbientColor` — `out Color`
- `AmbientIntensity` — `out double`
- `CameraLight` — `out OcctDirectionalLightSettings`
- `SunLight` — `out OcctDirectionalLightSettings`
- `FillLight` — `out OcctDirectionalLightSettings`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctSceneLightingSettings other)
```

**Parameters**

- `other` — `OcctSceneLightingSettings`

**Returns:** `bool`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual bool Equals(object obj)
```

**Parameters**

- `obj` — `object`

**Returns:** `bool`

### `GetHashCode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual int GetHashCode()
```

**Returns:** `int`

### `ToString`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual string ToString()
```

**Returns:** `string`

## Fields / Enum Values

None

