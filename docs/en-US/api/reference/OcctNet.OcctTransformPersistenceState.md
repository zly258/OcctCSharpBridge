# OcctTransformPersistenceState

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctTransformPersistenceState`

```csharp
public OcctTransformPersistenceState(OcctTransformPersistenceMode Mode, OcctPoint3d Anchor, OcctCornerPosition Position, int OffsetX, int OffsetY)
```

## Properties

### `Anchor`

```csharp
public OcctPoint3d Anchor { get; set; }
```

### `Enabled`

```csharp
public bool Enabled { get; }
```

### `IsScreenAnchored`

```csharp
public bool IsScreenAnchored { get; }
```

### `Mode`

```csharp
public OcctTransformPersistenceMode Mode { get; set; }
```

### `OffsetX`

```csharp
public int OffsetX { get; set; }
```

### `OffsetY`

```csharp
public int OffsetY { get; set; }
```

### `Position`

```csharp
public OcctCornerPosition Position { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(OcctTransformPersistenceMode Mode, OcctPoint3d Anchor, OcctCornerPosition Position, int OffsetX, int OffsetY)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTransformPersistenceState other)
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

