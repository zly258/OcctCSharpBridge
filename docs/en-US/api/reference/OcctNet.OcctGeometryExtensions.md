# OcctGeometryExtensions

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public static class OcctGeometryExtensions
```

## Description

Pure managed geometry helpers for common CAD calculations that do not require a native OCCT call.

## Constructors

None

## Properties

None

## Events

None

## Methods

### `AlmostEquals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool AlmostEquals(OcctPoint3d first, OcctPoint3d second, double tolerance = 1E-09)
```

**Parameters**

- `first` — `OcctPoint3d`
- `second` — `OcctPoint3d`
- `tolerance` — `double` = 1E-09

**Returns:** `bool`

### `AlmostEquals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool AlmostEquals(OcctVector3d first, OcctVector3d second, double tolerance = 1E-09)
```

**Parameters**

- `first` — `OcctVector3d`
- `second` — `OcctVector3d`
- `tolerance` — `double` = 1E-09

**Returns:** `bool`

### `AngleTo`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static double AngleTo(OcctVector3d first, OcctVector3d second)
```

**Parameters**

- `first` — `OcctVector3d`
- `second` — `OcctVector3d`

**Returns:** `double`

### `Contains`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool Contains(OcctBounds bounds, OcctPoint3d point, double tolerance = 0)
```

**Parameters**

- `bounds` — `OcctBounds`
- `point` — `OcctPoint3d`
- `tolerance` — `double` = 0

**Returns:** `bool`

### `Contains`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool Contains(OcctUvBounds bounds, double u, double v, double tolerance = 0)
```

**Parameters**

- `bounds` — `OcctUvBounds`
- `u` — `double`
- `v` — `double`
- `tolerance` — `double` = 0

**Returns:** `bool`

### `CreateRotationLocation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctModelLocation CreateRotationLocation(OcctVector3d axis, double angleRadians, OcctPoint3d center = null)
```

**Parameters**

- `axis` — `OcctVector3d`
- `angleRadians` — `double`
- `center` — `OcctPoint3d` = null

**Returns:** `OcctModelLocation`

### `CreateRotationTransform`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctTransform3d CreateRotationTransform(OcctVector3d axis, double angleRadians, OcctPoint3d center = null)
```

**Parameters**

- `axis` — `OcctVector3d`
- `angleRadians` — `double`
- `center` — `OcctPoint3d` = null

**Returns:** `OcctTransform3d`

### `CreateTranslationLocation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctModelLocation CreateTranslationLocation(double x, double y, double z)
```

**Parameters**

- `x` — `double`
- `y` — `double`
- `z` — `double`

**Returns:** `OcctModelLocation`

### `CreateUniformScaleLocation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctModelLocation CreateUniformScaleLocation(double scale, OcctPoint3d center = null)
```

**Parameters**

- `scale` — `double`
- `center` — `OcctPoint3d` = null

**Returns:** `OcctModelLocation`

### `CreateUniformScaleTransform`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctTransform3d CreateUniformScaleTransform(double scale, OcctPoint3d center = null)
```

**Parameters**

- `scale` — `double`
- `center` — `OcctPoint3d` = null

**Returns:** `OcctTransform3d`

### `Expanded`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctBounds Expanded(OcctBounds bounds, double margin)
```

**Parameters**

- `bounds` — `OcctBounds`
- `margin` — `double`

**Returns:** `OcctBounds`

### `GetCenter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static ValueTuple<double, double> GetCenter(OcctUvBounds bounds)
```

**Parameters**

- `bounds` — `OcctUvBounds`

**Returns:** `ValueTuple<double, double>`

### `GetDiagonalLength`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static double GetDiagonalLength(OcctBounds bounds)
```

**Parameters**

- `bounds` — `OcctBounds`

**Returns:** `double`

### `GetMaximumPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctPoint3d GetMaximumPoint(OcctBounds bounds)
```

**Parameters**

- `bounds` — `OcctBounds`

**Returns:** `OcctPoint3d`

### `GetMidpoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctPoint3d GetMidpoint(OcctDistanceResult result)
```

**Parameters**

- `result` — `OcctDistanceResult`

**Returns:** `OcctPoint3d`

### `GetMinimumPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctPoint3d GetMinimumPoint(OcctBounds bounds)
```

**Parameters**

- `bounds` — `OcctBounds`

**Returns:** `OcctPoint3d`

### `GetSeparationVector`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctVector3d GetSeparationVector(OcctDistanceResult result)
```

**Parameters**

- `result` — `OcctDistanceResult`

**Returns:** `OcctVector3d`

### `GetVolume`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static double GetVolume(OcctBounds bounds)
```

**Parameters**

- `bounds` — `OcctBounds`

**Returns:** `double`

### `Intersects`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool Intersects(OcctBounds first, OcctBounds second, double tolerance = 0)
```

**Parameters**

- `first` — `OcctBounds`
- `second` — `OcctBounds`
- `tolerance` — `double` = 0

**Returns:** `bool`

### `Inverted`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctModelLocation Inverted(OcctModelLocation transform)
```

**Parameters**

- `transform` — `OcctModelLocation`

**Returns:** `OcctModelLocation`

### `IsAffine`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsAffine(OcctModelLocation transform, double tolerance = 1E-12)
```

**Parameters**

- `transform` — `OcctModelLocation`
- `tolerance` — `double` = 1E-12

**Returns:** `bool`

### `IsFinite`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsFinite(OcctBounds bounds)
```

**Parameters**

- `bounds` — `OcctBounds`

**Returns:** `bool`

### `IsFinite`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsFinite(OcctDistanceResult result)
```

**Parameters**

- `result` — `OcctDistanceResult`

**Returns:** `bool`

### `IsFinite`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsFinite(OcctUvBounds bounds)
```

**Parameters**

- `bounds` — `OcctUvBounds`

**Returns:** `bool`

### `IsValid`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsValid(OcctBounds bounds)
```

**Parameters**

- `bounds` — `OcctBounds`

**Returns:** `bool`

### `IsValid`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsValid(OcctUvBounds bounds)
```

**Parameters**

- `bounds` — `OcctUvBounds`

**Returns:** `bool`

### `IsWithin`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool IsWithin(OcctDistanceResult result, double tolerance)
```

**Parameters**

- `result` — `OcctDistanceResult`
- `tolerance` — `double`

**Returns:** `bool`

### `Lerp`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctPoint3d Lerp(OcctPoint3d from, OcctPoint3d to, double amount)
```

**Parameters**

- `from` — `OcctPoint3d`
- `to` — `OcctPoint3d`
- `amount` — `double`

**Returns:** `OcctPoint3d`

### `Multiply`

Returns left × right. With column-vector semantics, right is applied first and left second.

```csharp
public static OcctModelLocation Multiply(OcctModelLocation left, OcctModelLocation right)
```

**Parameters**

- `left` — `OcctModelLocation`
- `right` — `OcctModelLocation`

**Returns:** `OcctModelLocation`

### `Multiply`

Returns left × right. With column-vector semantics, right is applied first and left second.

```csharp
public static OcctTransform3d Multiply(OcctTransform3d left, OcctTransform3d right)
```

**Parameters**

- `left` — `OcctTransform3d`
- `right` — `OcctTransform3d`

**Returns:** `OcctTransform3d`

### `ProjectOnto`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctVector3d ProjectOnto(OcctVector3d vector, OcctVector3d axis)
```

**Parameters**

- `vector` — `OcctVector3d`
- `axis` — `OcctVector3d`

**Returns:** `OcctVector3d`

### `RejectFrom`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctVector3d RejectFrom(OcctVector3d vector, OcctVector3d axis)
```

**Parameters**

- `vector` — `OcctVector3d`
- `axis` — `OcctVector3d`

**Returns:** `OcctVector3d`

### `ToModelLocation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctModelLocation ToModelLocation(OcctTransform3d transform)
```

**Parameters**

- `transform` — `OcctTransform3d`

**Returns:** `OcctModelLocation`

### `ToTransform3d`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctTransform3d ToTransform3d(OcctModelLocation transform)
```

**Parameters**

- `transform` — `OcctModelLocation`

**Returns:** `OcctTransform3d`

### `TransformPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctPoint3d TransformPoint(OcctModelLocation transform, OcctPoint3d point)
```

**Parameters**

- `transform` — `OcctModelLocation`
- `point` — `OcctPoint3d`

**Returns:** `OcctPoint3d`

### `TransformPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctPoint3d TransformPoint(OcctTransform3d transform, OcctPoint3d point)
```

**Parameters**

- `transform` — `OcctTransform3d`
- `point` — `OcctPoint3d`

**Returns:** `OcctPoint3d`

### `TransformVector`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctVector3d TransformVector(OcctModelLocation transform, OcctVector3d vector)
```

**Parameters**

- `transform` — `OcctModelLocation`
- `vector` — `OcctVector3d`

**Returns:** `OcctVector3d`

### `TransformVector`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctVector3d TransformVector(OcctTransform3d transform, OcctVector3d vector)
```

**Parameters**

- `transform` — `OcctTransform3d`
- `vector` — `OcctVector3d`

**Returns:** `OcctVector3d`

### `TryInvert`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static bool TryInvert(OcctModelLocation transform, out OcctModelLocation inverse)
```

**Parameters**

- `transform` — `OcctModelLocation`
- `inverse` — `out OcctModelLocation`

**Returns:** `bool`

### `Union`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctBounds Union(OcctBounds first, OcctBounds second)
```

**Parameters**

- `first` — `OcctBounds`
- `second` — `OcctBounds`

**Returns:** `OcctBounds`

## Fields / Enum Values

None

