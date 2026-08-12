# OcctGeometryExtensions

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public static class OcctGeometryExtensions
```

## 说明

Pure managed geometry helpers for common CAD calculations that do not require a native OCCT call.

## 构造函数

无

## 属性

无

## 事件

无

## 方法

### `AlmostEquals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool AlmostEquals(OcctPoint3d first, OcctPoint3d second, double tolerance = 1E-09)
```

**参数**

- `first` — `OcctPoint3d`
- `second` — `OcctPoint3d`
- `tolerance` — `double` = 1E-09

**返回值:** `bool`

### `AlmostEquals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool AlmostEquals(OcctVector3d first, OcctVector3d second, double tolerance = 1E-09)
```

**参数**

- `first` — `OcctVector3d`
- `second` — `OcctVector3d`
- `tolerance` — `double` = 1E-09

**返回值:** `bool`

### `AngleTo`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static double AngleTo(OcctVector3d first, OcctVector3d second)
```

**参数**

- `first` — `OcctVector3d`
- `second` — `OcctVector3d`

**返回值:** `double`

### `Contains`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool Contains(OcctBounds bounds, OcctPoint3d point, double tolerance = 0)
```

**参数**

- `bounds` — `OcctBounds`
- `point` — `OcctPoint3d`
- `tolerance` — `double` = 0

**返回值:** `bool`

### `Contains`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool Contains(OcctUvBounds bounds, double u, double v, double tolerance = 0)
```

**参数**

- `bounds` — `OcctUvBounds`
- `u` — `double`
- `v` — `double`
- `tolerance` — `double` = 0

**返回值:** `bool`

### `CreateRotationLocation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctModelLocation CreateRotationLocation(OcctVector3d axis, double angleRadians, OcctPoint3d center = null)
```

**参数**

- `axis` — `OcctVector3d`
- `angleRadians` — `double`
- `center` — `OcctPoint3d` = null

**返回值:** `OcctModelLocation`

### `CreateRotationTransform`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctTransform3d CreateRotationTransform(OcctVector3d axis, double angleRadians, OcctPoint3d center = null)
```

**参数**

- `axis` — `OcctVector3d`
- `angleRadians` — `double`
- `center` — `OcctPoint3d` = null

**返回值:** `OcctTransform3d`

### `CreateTranslationLocation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctModelLocation CreateTranslationLocation(double x, double y, double z)
```

**参数**

- `x` — `double`
- `y` — `double`
- `z` — `double`

**返回值:** `OcctModelLocation`

### `CreateUniformScaleLocation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctModelLocation CreateUniformScaleLocation(double scale, OcctPoint3d center = null)
```

**参数**

- `scale` — `double`
- `center` — `OcctPoint3d` = null

**返回值:** `OcctModelLocation`

### `CreateUniformScaleTransform`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctTransform3d CreateUniformScaleTransform(double scale, OcctPoint3d center = null)
```

**参数**

- `scale` — `double`
- `center` — `OcctPoint3d` = null

**返回值:** `OcctTransform3d`

### `Expanded`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctBounds Expanded(OcctBounds bounds, double margin)
```

**参数**

- `bounds` — `OcctBounds`
- `margin` — `double`

**返回值:** `OcctBounds`

### `GetCenter`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static ValueTuple<double, double> GetCenter(OcctUvBounds bounds)
```

**参数**

- `bounds` — `OcctUvBounds`

**返回值:** `ValueTuple<double, double>`

### `GetDiagonalLength`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static double GetDiagonalLength(OcctBounds bounds)
```

**参数**

- `bounds` — `OcctBounds`

**返回值:** `double`

### `GetMaximumPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctPoint3d GetMaximumPoint(OcctBounds bounds)
```

**参数**

- `bounds` — `OcctBounds`

**返回值:** `OcctPoint3d`

### `GetMidpoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctPoint3d GetMidpoint(OcctDistanceResult result)
```

**参数**

- `result` — `OcctDistanceResult`

**返回值:** `OcctPoint3d`

### `GetMinimumPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctPoint3d GetMinimumPoint(OcctBounds bounds)
```

**参数**

- `bounds` — `OcctBounds`

**返回值:** `OcctPoint3d`

### `GetSeparationVector`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctVector3d GetSeparationVector(OcctDistanceResult result)
```

**参数**

- `result` — `OcctDistanceResult`

**返回值:** `OcctVector3d`

### `GetVolume`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static double GetVolume(OcctBounds bounds)
```

**参数**

- `bounds` — `OcctBounds`

**返回值:** `double`

### `Intersects`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool Intersects(OcctBounds first, OcctBounds second, double tolerance = 0)
```

**参数**

- `first` — `OcctBounds`
- `second` — `OcctBounds`
- `tolerance` — `double` = 0

**返回值:** `bool`

### `Inverted`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctModelLocation Inverted(OcctModelLocation transform)
```

**参数**

- `transform` — `OcctModelLocation`

**返回值:** `OcctModelLocation`

### `IsAffine`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsAffine(OcctModelLocation transform, double tolerance = 1E-12)
```

**参数**

- `transform` — `OcctModelLocation`
- `tolerance` — `double` = 1E-12

**返回值:** `bool`

### `IsFinite`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsFinite(OcctBounds bounds)
```

**参数**

- `bounds` — `OcctBounds`

**返回值:** `bool`

### `IsFinite`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsFinite(OcctDistanceResult result)
```

**参数**

- `result` — `OcctDistanceResult`

**返回值:** `bool`

### `IsFinite`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsFinite(OcctUvBounds bounds)
```

**参数**

- `bounds` — `OcctUvBounds`

**返回值:** `bool`

### `IsValid`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsValid(OcctBounds bounds)
```

**参数**

- `bounds` — `OcctBounds`

**返回值:** `bool`

### `IsValid`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsValid(OcctUvBounds bounds)
```

**参数**

- `bounds` — `OcctUvBounds`

**返回值:** `bool`

### `IsWithin`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool IsWithin(OcctDistanceResult result, double tolerance)
```

**参数**

- `result` — `OcctDistanceResult`
- `tolerance` — `double`

**返回值:** `bool`

### `Lerp`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctPoint3d Lerp(OcctPoint3d from, OcctPoint3d to, double amount)
```

**参数**

- `from` — `OcctPoint3d`
- `to` — `OcctPoint3d`
- `amount` — `double`

**返回值:** `OcctPoint3d`

### `Multiply`

Returns left × right. With column-vector semantics, right is applied first and left second.

```csharp
public static OcctModelLocation Multiply(OcctModelLocation left, OcctModelLocation right)
```

**参数**

- `left` — `OcctModelLocation`
- `right` — `OcctModelLocation`

**返回值:** `OcctModelLocation`

### `Multiply`

Returns left × right. With column-vector semantics, right is applied first and left second.

```csharp
public static OcctTransform3d Multiply(OcctTransform3d left, OcctTransform3d right)
```

**参数**

- `left` — `OcctTransform3d`
- `right` — `OcctTransform3d`

**返回值:** `OcctTransform3d`

### `ProjectOnto`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctVector3d ProjectOnto(OcctVector3d vector, OcctVector3d axis)
```

**参数**

- `vector` — `OcctVector3d`
- `axis` — `OcctVector3d`

**返回值:** `OcctVector3d`

### `RejectFrom`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctVector3d RejectFrom(OcctVector3d vector, OcctVector3d axis)
```

**参数**

- `vector` — `OcctVector3d`
- `axis` — `OcctVector3d`

**返回值:** `OcctVector3d`

### `ToModelLocation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctModelLocation ToModelLocation(OcctTransform3d transform)
```

**参数**

- `transform` — `OcctTransform3d`

**返回值:** `OcctModelLocation`

### `ToTransform3d`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctTransform3d ToTransform3d(OcctModelLocation transform)
```

**参数**

- `transform` — `OcctModelLocation`

**返回值:** `OcctTransform3d`

### `TransformPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctPoint3d TransformPoint(OcctModelLocation transform, OcctPoint3d point)
```

**参数**

- `transform` — `OcctModelLocation`
- `point` — `OcctPoint3d`

**返回值:** `OcctPoint3d`

### `TransformPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctPoint3d TransformPoint(OcctTransform3d transform, OcctPoint3d point)
```

**参数**

- `transform` — `OcctTransform3d`
- `point` — `OcctPoint3d`

**返回值:** `OcctPoint3d`

### `TransformVector`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctVector3d TransformVector(OcctModelLocation transform, OcctVector3d vector)
```

**参数**

- `transform` — `OcctModelLocation`
- `vector` — `OcctVector3d`

**返回值:** `OcctVector3d`

### `TransformVector`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctVector3d TransformVector(OcctTransform3d transform, OcctVector3d vector)
```

**参数**

- `transform` — `OcctTransform3d`
- `vector` — `OcctVector3d`

**返回值:** `OcctVector3d`

### `TryInvert`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static bool TryInvert(OcctModelLocation transform, out OcctModelLocation inverse)
```

**参数**

- `transform` — `OcctModelLocation`
- `inverse` — `out OcctModelLocation`

**返回值:** `bool`

### `Union`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public static OcctBounds Union(OcctBounds first, OcctBounds second)
```

**参数**

- `first` — `OcctBounds`
- `second` — `OcctBounds`

**返回值:** `OcctBounds`

## 字段 / 枚举值

无

