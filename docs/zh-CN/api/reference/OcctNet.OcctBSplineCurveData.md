# OcctBSplineCurveData

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctBSplineCurveData
```

## 说明

Immutable snapshot of the OCCT B-Spline data carried by an edge curve. Pole and knot lists use zero-based managed indexing.

## 构造函数

无

## 属性

### `Degree`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int Degree { get; }
```

### `IsPeriodic`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsPeriodic { get; }
```

### `IsRational`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsRational { get; }
```

### `KnotCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int KnotCount { get; }
```

### `Knots`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<double> Knots { get; }
```

### `Multiplicities`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<int> Multiplicities { get; }
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

### `Weights`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<double> Weights { get; }
```

## 事件

无

## 方法

无

## 字段 / 枚举值

无

