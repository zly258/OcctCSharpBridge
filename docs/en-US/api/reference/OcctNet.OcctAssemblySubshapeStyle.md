# OcctAssemblySubshapeStyle

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

## Constructors

### `OcctAssemblySubshapeStyle`

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

```csharp
public OcctAssemblySubshapeStyle(OcctShapeType ShapeType, int SubshapeIndex, OcctAssemblyStyle Style)
```

## Properties

### `ShapeType`

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `Style`

```csharp
public OcctAssemblyStyle Style { get; set; }
```

### `SubshapeIndex`

```csharp
public int SubshapeIndex { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(OcctShapeType ShapeType, int SubshapeIndex, OcctAssemblyStyle Style)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctAssemblySubshapeStyle other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## Fields / Enum Values

None.

