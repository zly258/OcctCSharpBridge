# OcctDirectionalLightSettings

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctDirectionalLightSettings
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctDirectionalLightSettings`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDirectionalLightSettings(bool Enabled, Color Color, double Intensity, OcctVector3d Direction)
```

**Parameters**

- `Enabled` — `bool`
- `Color` — `Color`
- `Intensity` — `double`
- `Direction` — `OcctVector3d`

## Properties

### `Color`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public Color Color { get; set; }
```

### `Direction`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Direction { get; set; }
```

### `Enabled`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Enabled { get; set; }
```

### `Intensity`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Intensity { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out bool Enabled, out Color Color, out double Intensity, out OcctVector3d Direction)
```

**Parameters**

- `Enabled` — `out bool`
- `Color` — `out Color`
- `Intensity` — `out double`
- `Direction` — `out OcctVector3d`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctDirectionalLightSettings other)
```

**Parameters**

- `other` — `OcctDirectionalLightSettings`

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

