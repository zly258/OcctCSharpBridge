# OcctBSplineSurfaceData

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctBSplineSurfaceData
```

## 说明

Immutable snapshot of the OCCT B-Spline data carried by a face surface. Pole and weight grids use zero-based managed indexing. Flat pole storage is U-major with V varying fastest: flatIndex = uIndex * VPoleCount + vIndex.

## 构造函数

无

## 属性

### `IsUPeriodic`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsUPeriodic { get; }
```

### `IsURational`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsURational { get; }
```

### `IsVPeriodic`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsVPeriodic { get; }
```

### `IsVRational`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsVRational { get; }
```

### `PoleCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int PoleCount { get; }
```

### `Poles`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctPoint3d> Poles { get; }
```

### `UDegree`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int UDegree { get; }
```

### `UKnotCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int UKnotCount { get; }
```

### `UKnots`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<double> UKnots { get; }
```

### `UMultiplicities`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<int> UMultiplicities { get; }
```

### `UPoleCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int UPoleCount { get; }
```

### `VDegree`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int VDegree { get; }
```

### `VKnotCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int VKnotCount { get; }
```

### `VKnots`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<double> VKnots { get; }
```

### `VMultiplicities`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<int> VMultiplicities { get; }
```

### `VPoleCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int VPoleCount { get; }
```

### `Weights`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<double> Weights { get; }
```

## 事件

无

## 方法

### `GetPole`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d GetPole(int uIndex, int vIndex)
```

**参数**

- `uIndex` — `int`
- `vIndex` — `int`

**返回值:** `OcctPoint3d`

### `GetWeight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double GetWeight(int uIndex, int vIndex)
```

**参数**

- `uIndex` — `int`
- `vIndex` — `int`

**返回值:** `double`

## 字段 / 枚举值

无

