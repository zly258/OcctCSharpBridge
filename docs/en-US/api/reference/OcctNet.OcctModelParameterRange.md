# OcctModelParameterRange

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelParameterRange
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelParameterRange`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelParameterRange(double FirstParameter, double LastParameter, bool IsClosed, bool IsPeriodic, double Period)
```

**Parameters**

- `FirstParameter` — `double`
- `LastParameter` — `double`
- `IsClosed` — `bool`
- `IsPeriodic` — `bool`
- `Period` — `double`

## Properties

### `FirstParameter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double FirstParameter { get; set; }
```

### `IsClosed`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsClosed { get; set; }
```

### `IsPeriodic`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsPeriodic { get; set; }
```

### `LastParameter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double LastParameter { get; set; }
```

### `Length`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Length { get; }
```

### `Period`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Period { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double FirstParameter, out double LastParameter, out bool IsClosed, out bool IsPeriodic, out double Period)
```

**Parameters**

- `FirstParameter` — `out double`
- `LastParameter` — `out double`
- `IsClosed` — `out bool`
- `IsPeriodic` — `out bool`
- `Period` — `out double`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelParameterRange other)
```

**Parameters**

- `other` — `OcctModelParameterRange`

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

