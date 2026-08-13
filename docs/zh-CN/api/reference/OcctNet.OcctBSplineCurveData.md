# OcctBSplineCurveData

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Immutable snapshot of the OCCT B-Spline data carried by an edge curve. Pole and knot lists use zero-based managed indexing.

## 构造函数

无。

## 属性

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

## 事件

无。

## 方法

无。

## 字段 / 枚举值

无。

