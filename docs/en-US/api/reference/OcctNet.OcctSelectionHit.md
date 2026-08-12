# OcctSelectionHit

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctSelectionHit
```

## Description

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

## Constructors

### `OcctSelectionHit`

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

```csharp
public OcctSelectionHit(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex)
```

**Parameters**

- `Owner` — `IOcctObject`
- `SubshapeType` — `OcctShapeType`
- `SubshapeIndex` — `int`

## Properties

### `IsSubshape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsSubshape { get; }
```

### `Owner`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IOcctObject Owner { get; set; }
```

### `SubshapeIndex`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int SubshapeIndex { get; set; }
```

### `SubshapeType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShapeType SubshapeType { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out IOcctObject Owner, out OcctShapeType SubshapeType, out int SubshapeIndex)
```

**Parameters**

- `Owner` — `out IOcctObject`
- `SubshapeType` — `out OcctShapeType`
- `SubshapeIndex` — `out int`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctSelectionHit other)
```

**Parameters**

- `other` — `OcctSelectionHit`

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

