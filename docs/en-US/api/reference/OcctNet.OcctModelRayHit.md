# OcctModelRayHit

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelRayHit
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelRayHit`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelRayHit(OcctPoint3d Point, OcctModelShape Face, double RayParameter, double U, double V, OcctModelState State)
```

**Parameters**

- `Point` — `OcctPoint3d`
- `Face` — `OcctModelShape`
- `RayParameter` — `double`
- `U` — `double`
- `V` — `double`
- `State` — `OcctModelState`

## Properties

### `Face`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Face { get; set; }
```

### `Point`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Point { get; set; }
```

### `RayParameter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RayParameter { get; set; }
```

### `State`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelState State { get; set; }
```

### `U`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double U { get; set; }
```

### `V`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double V { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctPoint3d Point, out OcctModelShape Face, out double RayParameter, out double U, out double V, out OcctModelState State)
```

**Parameters**

- `Point` — `out OcctPoint3d`
- `Face` — `out OcctModelShape`
- `RayParameter` — `out double`
- `U` — `out double`
- `V` — `out double`
- `State` — `out OcctModelState`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelRayHit other)
```

**Parameters**

- `other` — `OcctModelRayHit`

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

