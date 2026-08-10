# OcctPoint3d

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctPoint3d
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctPoint3d`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d(double x, double y, double z)
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

### `Origin`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Origin { get; }
```

## Events

None

## Methods

### `DistanceTo`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double DistanceTo(OcctPoint3d other)
```

**Parameters**

- `other` — `OcctPoint3d`

**Returns:** `double`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctPoint3d other)
```

**Parameters**

- `other` — `OcctPoint3d`

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

- `X` — `double` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `Y` — `double` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `Z` — `double` — Public API member. Exact parameters, return type, and available XML documentation are listed below.

