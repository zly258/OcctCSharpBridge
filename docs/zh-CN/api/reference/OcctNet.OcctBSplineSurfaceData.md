# OcctBSplineSurfaceData

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Immutable snapshot of the OCCT B-Spline data carried by a face surface. Pole and weight grids use zero-based managed indexing. Flat pole storage is U-major with V varying fastest: flatIndex = uIndex * VPoleCount + vIndex.

## 构造函数

无。

## 属性

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

## 事件

无。

## 方法

### `GetPole`

```csharp
public OcctPoint3d GetPole(int uIndex, int vIndex)
```

### `GetWeight`

```csharp
public double GetWeight(int uIndex, int vIndex)
```

## 字段 / 枚举值

无。

