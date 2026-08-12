# OcctPolygonOffsetSettings

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctPolygonOffsetSettings
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctPolygonOffsetSettings`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPolygonOffsetSettings(OcctPolygonOffsetMode Mode, double Factor, double Units)
```

**Parameters**

- `Mode` — `OcctPolygonOffsetMode`
- `Factor` — `double`
- `Units` — `double`

## Properties

### `Factor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Factor { get; set; }
```

### `Mode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPolygonOffsetMode Mode { get; set; }
```

### `Units`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Units { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctPolygonOffsetMode Mode, out double Factor, out double Units)
```

**Parameters**

- `Mode` — `out OcctPolygonOffsetMode`
- `Factor` — `out double`
- `Units` — `out double`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctPolygonOffsetSettings other)
```

**Parameters**

- `other` — `OcctPolygonOffsetSettings`

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

