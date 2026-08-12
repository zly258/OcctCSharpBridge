# OcctBSplineSurfaceData

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctBSplineSurfaceData
```

## Description

Immutable snapshot of the OCCT B-Spline data carried by a face surface. Pole and weight grids use zero-based managed indexing. Flat pole storage is U-major with V varying fastest: flatIndex = uIndex * VPoleCount + vIndex.

## Constructors

None

## Properties

### `IsUPeriodic`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsUPeriodic { get; }
```

### `IsURational`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsURational { get; }
```

### `IsVPeriodic`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsVPeriodic { get; }
```

### `IsVRational`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsVRational { get; }
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

### `UDegree`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int UDegree { get; }
```

### `UKnotCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int UKnotCount { get; }
```

### `UKnots`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<double> UKnots { get; }
```

### `UMultiplicities`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<int> UMultiplicities { get; }
```

### `UPoleCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int UPoleCount { get; }
```

### `VDegree`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int VDegree { get; }
```

### `VKnotCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int VKnotCount { get; }
```

### `VKnots`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<double> VKnots { get; }
```

### `VMultiplicities`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<int> VMultiplicities { get; }
```

### `VPoleCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int VPoleCount { get; }
```

### `Weights`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<double> Weights { get; }
```

## Events

None

## Methods

### `GetPole`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d GetPole(int uIndex, int vIndex)
```

**Parameters**

- `uIndex` — `int`
- `vIndex` — `int`

**Returns:** `OcctPoint3d`

### `GetWeight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double GetWeight(int uIndex, int vIndex)
```

**Parameters**

- `uIndex` — `int`
- `vIndex` — `int`

**Returns:** `double`

## Fields / Enum Values

None

