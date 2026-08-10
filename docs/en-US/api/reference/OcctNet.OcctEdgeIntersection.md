# OcctEdgeIntersection

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctEdgeIntersection
```

## Description

A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values. For a point intersection, start and end points/parameters are equal.

## Constructors

### `OcctEdgeIntersection`

A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values. For a point intersection, start and end points/parameters are equal.

```csharp
public OcctEdgeIntersection(OcctIntersectionKind Kind, OcctPoint3d StartPoint, OcctPoint3d EndPoint, double FirstParameterStart, double FirstParameterEnd, double SecondParameterStart, double SecondParameterEnd)
```

**Parameters**

- `Kind` — `OcctIntersectionKind`
- `StartPoint` — `OcctPoint3d`
- `EndPoint` — `OcctPoint3d`
- `FirstParameterStart` — `double`
- `FirstParameterEnd` — `double`
- `SecondParameterStart` — `double`
- `SecondParameterEnd` — `double`

## Properties

### `EndPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d EndPoint { get; set; }
```

### `FirstParameterEnd`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double FirstParameterEnd { get; set; }
```

### `FirstParameterStart`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double FirstParameterStart { get; set; }
```

### `Kind`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctIntersectionKind Kind { get; set; }
```

### `SecondParameterEnd`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double SecondParameterEnd { get; set; }
```

### `SecondParameterStart`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double SecondParameterStart { get; set; }
```

### `StartPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d StartPoint { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctIntersectionKind Kind, out OcctPoint3d StartPoint, out OcctPoint3d EndPoint, out double FirstParameterStart, out double FirstParameterEnd, out double SecondParameterStart, out double SecondParameterEnd)
```

**Parameters**

- `Kind` — `out OcctIntersectionKind`
- `StartPoint` — `out OcctPoint3d`
- `EndPoint` — `out OcctPoint3d`
- `FirstParameterStart` — `out double`
- `FirstParameterEnd` — `out double`
- `SecondParameterStart` — `out double`
- `SecondParameterEnd` — `out double`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctEdgeIntersection other)
```

**Parameters**

- `other` — `OcctEdgeIntersection`

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

