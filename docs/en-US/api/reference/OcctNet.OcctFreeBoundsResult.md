# OcctFreeBoundsResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Read-only result of strict OCCT free-boundary analysis. Closed and open wires are owned by the same modeling session as the analyzed shape.

## Constructors

None.

## Properties

### `ClosedWireCount`

```csharp
public int ClosedWireCount { get; }
```

### `ClosedWires`

```csharp
public IReadOnlyList<OcctModelShape> ClosedWires { get; }
```

### `HasFreeBounds`

```csharp
public bool HasFreeBounds { get; }
```

### `HasOpenFreeBounds`

```csharp
public bool HasOpenFreeBounds { get; }
```

### `OpenWireCount`

```csharp
public int OpenWireCount { get; }
```

### `OpenWires`

```csharp
public IReadOnlyList<OcctModelShape> OpenWires { get; }
```

### `Tolerance`

```csharp
public double Tolerance { get; }
```

### `TotalWireCount`

```csharp
public int TotalWireCount { get; }
```

## Events

None.

## Methods

None.

## Fields / Enum Values

None.

