# OcctFaceEvaluation

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctFaceEvaluation
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctFaceEvaluation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctFaceEvaluation(OcctPoint3d Point, OcctVector3d Normal)
```

**Parameters**

- `Point` — `OcctPoint3d`
- `Normal` — `OcctVector3d`

## Properties

### `Normal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Normal { get; set; }
```

### `Point`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Point { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctPoint3d Point, out OcctVector3d Normal)
```

**Parameters**

- `Point` — `out OcctPoint3d`
- `Normal` — `out OcctVector3d`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctFaceEvaluation other)
```

**Parameters**

- `other` — `OcctFaceEvaluation`

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

