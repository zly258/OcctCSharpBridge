# OcctModelCurveDifferential

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelCurveDifferential
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelCurveDifferential`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelCurveDifferential(double Parameter, OcctPoint3d Point, OcctVector3d FirstDerivative, OcctVector3d SecondDerivative)
```

**Parameters**

- `Parameter` — `double`
- `Point` — `OcctPoint3d`
- `FirstDerivative` — `OcctVector3d`
- `SecondDerivative` — `OcctVector3d`

## Properties

### `FirstDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d FirstDerivative { get; set; }
```

### `Parameter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Parameter { get; set; }
```

### `Point`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Point { get; set; }
```

### `SecondDerivative`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d SecondDerivative { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double Parameter, out OcctPoint3d Point, out OcctVector3d FirstDerivative, out OcctVector3d SecondDerivative)
```

**Parameters**

- `Parameter` — `out double`
- `Point` — `out OcctPoint3d`
- `FirstDerivative` — `out OcctVector3d`
- `SecondDerivative` — `out OcctVector3d`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelCurveDifferential other)
```

**Parameters**

- `other` — `OcctModelCurveDifferential`

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

