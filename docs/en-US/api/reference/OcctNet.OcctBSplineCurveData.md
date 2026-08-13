# OcctBSplineCurveData

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Immutable snapshot of the OCCT B-Spline data carried by an edge curve. Pole and knot lists use zero-based managed indexing.

## Constructors

None.

## Properties

### `Degree`

```csharp
public int Degree { get; }
```

### `IsPeriodic`

```csharp
public bool IsPeriodic { get; }
```

### `IsRational`

```csharp
public bool IsRational { get; }
```

### `KnotCount`

```csharp
public int KnotCount { get; }
```

### `Knots`

```csharp
public IReadOnlyList<double> Knots { get; }
```

### `Multiplicities`

```csharp
public IReadOnlyList<int> Multiplicities { get; }
```

### `PoleCount`

```csharp
public int PoleCount { get; }
```

### `Poles`

```csharp
public IReadOnlyList<OcctPoint3d> Poles { get; }
```

### `Weights`

```csharp
public IReadOnlyList<double> Weights { get; }
```

## Events

None.

## Methods

None.

## Fields / Enum Values

None.

