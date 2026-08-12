# OcctAssemblySubshapeStyle

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctAssemblySubshapeStyle
```

## Description

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

## Constructors

### `OcctAssemblySubshapeStyle`

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

```csharp
public OcctAssemblySubshapeStyle(OcctShapeType ShapeType, int SubshapeIndex, OcctAssemblyStyle Style)
```

**Parameters**

- `ShapeType` — `OcctShapeType`
- `SubshapeIndex` — `int`
- `Style` — `OcctAssemblyStyle`

## Properties

### `ShapeType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `Style`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctAssemblyStyle Style { get; set; }
```

### `SubshapeIndex`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int SubshapeIndex { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctShapeType ShapeType, out int SubshapeIndex, out OcctAssemblyStyle Style)
```

**Parameters**

- `ShapeType` — `out OcctShapeType`
- `SubshapeIndex` — `out int`
- `Style` — `out OcctAssemblyStyle`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctAssemblySubshapeStyle other)
```

**Parameters**

- `other` — `OcctAssemblySubshapeStyle`

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

