# OcctModelMeshNode

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctModelMeshNode
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctModelMeshNode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelMeshNode(OcctPoint3d Point, double U, double V, OcctVector3d Normal, bool HasUv, bool HasNormal)
```

**Parameters**

- `Point` — `OcctPoint3d`
- `U` — `double`
- `V` — `double`
- `Normal` — `OcctVector3d`
- `HasUv` — `bool`
- `HasNormal` — `bool`

## Properties

### `HasNormal`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasNormal { get; set; }
```

### `HasUv`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasUv { get; set; }
```

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
public void Deconstruct(out OcctPoint3d Point, out double U, out double V, out OcctVector3d Normal, out bool HasUv, out bool HasNormal)
```

**Parameters**

- `Point` — `out OcctPoint3d`
- `U` — `out double`
- `V` — `out double`
- `Normal` — `out OcctVector3d`
- `HasUv` — `out bool`
- `HasNormal` — `out bool`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctModelMeshNode other)
```

**Parameters**

- `other` — `OcctModelMeshNode`

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

