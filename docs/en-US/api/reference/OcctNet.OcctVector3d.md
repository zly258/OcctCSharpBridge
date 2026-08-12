# OcctVector3d

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctVector3d
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctVector3d`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d(double x, double y, double z)
```

**Parameters**

- `x` — `double`
- `y` — `double`
- `z` — `double`

## Properties

### `IsFinite`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsFinite { get; }
```

### `Length`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Length { get; }
```

### `LengthSquared`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double LengthSquared { get; }
```

### `UnitX`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d UnitX { get; }
```

### `UnitY`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d UnitY { get; }
```

### `UnitZ`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d UnitZ { get; }
```

### `Zero`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Zero { get; }
```

## Events

None

## Methods

### `Cross`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Cross(OcctVector3d other)
```

**Parameters**

- `other` — `OcctVector3d`

**Returns:** `OcctVector3d`

### `Dot`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Dot(OcctVector3d other)
```

**Parameters**

- `other` — `OcctVector3d`

**Returns:** `double`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctVector3d other)
```

**Parameters**

- `other` — `OcctVector3d`

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

### `Normalized`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Normalized()
```

**Returns:** `OcctVector3d`

### `ToString`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual string ToString()
```

**Returns:** `string`

### `TryNormalize`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryNormalize(out OcctVector3d result)
```

**Parameters**

- `result` — `out OcctVector3d`

**Returns:** `bool`

## Fields / Enum Values

- `X` — `double` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `Y` — `double` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `Z` — `double` — Public API member. Exact parameters, return type, and available XML documentation are listed below.

