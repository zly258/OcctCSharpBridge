# OcctSelectionHitDetail

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctSelectionHitDetail`

```csharp
public OcctSelectionHitDetail(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex, OcctPoint3d Point, double Depth, double DistanceToEye)
```

## Properties

### `Depth`

```csharp
public double Depth { get; set; }
```

### `DistanceToEye`

```csharp
public double DistanceToEye { get; set; }
```

### `IsSubshape`

```csharp
public bool IsSubshape { get; }
```

### `Owner`

```csharp
public IOcctObject Owner { get; set; }
```

### `Point`

```csharp
public OcctPoint3d Point { get; set; }
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
public void Deconstruct(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex, OcctPoint3d Point, double Depth, double DistanceToEye)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctSelectionHitDetail other)
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

