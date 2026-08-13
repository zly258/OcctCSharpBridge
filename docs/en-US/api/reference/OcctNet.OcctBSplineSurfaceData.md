# OcctBSplineSurfaceData

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Immutable snapshot of the OCCT B-Spline data carried by a face surface. Pole and weight grids use zero-based managed indexing. Flat pole storage is U-major with V varying fastest: flatIndex = uIndex * VPoleCount + vIndex.

## Constructors

None.

## Properties

### `IsUPeriodic`

```csharp
public bool IsUPeriodic { get; }
```

### `IsURational`

```csharp
public bool IsURational { get; }
```

### `IsVPeriodic`

```csharp
public bool IsVPeriodic { get; }
```

### `IsVRational`

```csharp
public bool IsVRational { get; }
```

### `PoleCount`

```csharp
public int PoleCount { get; }
```

### `Poles`

```csharp
public IReadOnlyList<OcctPoint3d> Poles { get; }
```

### `UDegree`

```csharp
public int UDegree { get; }
```

### `UKnotCount`

```csharp
public int UKnotCount { get; }
```

### `UKnots`

```csharp
public IReadOnlyList<double> UKnots { get; }
```

### `UMultiplicities`

```csharp
public IReadOnlyList<int> UMultiplicities { get; }
```

### `UPoleCount`

```csharp
public int UPoleCount { get; }
```

### `VDegree`

```csharp
public int VDegree { get; }
```

### `VKnotCount`

```csharp
public int VKnotCount { get; }
```

### `VKnots`

```csharp
public IReadOnlyList<double> VKnots { get; }
```

### `VMultiplicities`

```csharp
public IReadOnlyList<int> VMultiplicities { get; }
```

### `VPoleCount`

```csharp
public int VPoleCount { get; }
```

### `Weights`

```csharp
public IReadOnlyList<double> Weights { get; }
```

## Events

None.

## Methods

### `GetPole`

```csharp
public OcctPoint3d GetPole(int uIndex, int vIndex)
```

### `GetWeight`

```csharp
public double GetWeight(int uIndex, int vIndex)
```

## Fields / Enum Values

None.

