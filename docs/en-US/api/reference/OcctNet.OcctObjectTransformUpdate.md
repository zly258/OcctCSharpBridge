# OcctObjectTransformUpdate

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctObjectTransformUpdate
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctObjectTransformUpdate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctObjectTransformUpdate(IOcctObject Object, OcctTransform3d Transformation)
```

**Parameters**

- `Object` — `IOcctObject`
- `Transformation` — `OcctTransform3d`

## Properties

### `Object`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IOcctObject Object { get; set; }
```

### `Transformation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTransform3d Transformation { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out IOcctObject Object, out OcctTransform3d Transformation)
```

**Parameters**

- `Object` — `out IOcctObject`
- `Transformation` — `out OcctTransform3d`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctObjectTransformUpdate other)
```

**Parameters**

- `other` — `OcctObjectTransformUpdate`

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

