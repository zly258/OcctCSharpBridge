# OcctBSplineCurveData

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctBSplineCurveData
```

## Description

Immutable snapshot of the OCCT B-Spline data carried by an edge curve. Pole and knot lists use zero-based managed indexing.

## Constructors

None

## Properties

### `Degree`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int Degree { get; }
```

### `IsPeriodic`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsPeriodic { get; }
```

### `IsRational`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsRational { get; }
```

### `KnotCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int KnotCount { get; }
```

### `Knots`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<double> Knots { get; }
```

### `Multiplicities`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<int> Multiplicities { get; }
```

### `PoleCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int PoleCount { get; }
```

### `Poles`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctPoint3d> Poles { get; }
```

### `Weights`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<double> Weights { get; }
```

## Events

None

## Methods

None

## Fields / Enum Values

None

