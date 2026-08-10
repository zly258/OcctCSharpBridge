# OcctEdgeEvaluation

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctEdgeEvaluation
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctEdgeEvaluation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctEdgeEvaluation(OcctPoint3d Point, OcctVector3d Tangent)
```

**Parameters**

- `Point` — `OcctPoint3d`
- `Tangent` — `OcctVector3d`

## Properties

### `Point`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d Point { get; set; }
```

### `Tangent`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctVector3d Tangent { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctPoint3d Point, out OcctVector3d Tangent)
```

**Parameters**

- `Point` — `out OcctPoint3d`
- `Tangent` — `out OcctVector3d`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctEdgeEvaluation other)
```

**Parameters**

- `other` — `OcctEdgeEvaluation`

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

