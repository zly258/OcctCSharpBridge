# OcctSelectionHit

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

## Constructors

### `OcctSelectionHit`

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

```csharp
public OcctSelectionHit(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex)
```

## Properties

### `IsSubshape`

```csharp
public bool IsSubshape { get; }
```

### `Owner`

```csharp
public IOcctObject Owner { get; set; }
```

### `SubshapeIndex`

```csharp
public int SubshapeIndex { get; set; }
```

### `SubshapeType`

```csharp
public OcctShapeType SubshapeType { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctSelectionHit other)
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

