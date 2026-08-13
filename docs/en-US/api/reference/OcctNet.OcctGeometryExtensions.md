# OcctGeometryExtensions

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Pure managed geometry helpers for common CAD calculations that do not require a native OCCT call.

## Constructors

None.

## Properties

None.

## Events

None.

## Methods

### `AlmostEquals`

```csharp
public static bool AlmostEquals(OcctPoint3d first, OcctPoint3d second, double tolerance)
```

### `AlmostEquals`

```csharp
public static bool AlmostEquals(OcctVector3d first, OcctVector3d second, double tolerance)
```

### `AngleTo`

```csharp
public static double AngleTo(OcctVector3d first, OcctVector3d second)
```

### `Contains`

```csharp
public static bool Contains(OcctBounds bounds, OcctPoint3d point, double tolerance)
```

### `Contains`

```csharp
public static bool Contains(OcctUvBounds bounds, double u, double v, double tolerance)
```

### `CreateRotationLocation`

```csharp
public static OcctModelLocation CreateRotationLocation(OcctVector3d axis, double angleRadians, OcctPoint3d center)
```

### `CreateRotationTransform`

```csharp
public static OcctTransform3d CreateRotationTransform(OcctVector3d axis, double angleRadians, OcctPoint3d center)
```

### `CreateTranslationLocation`

```csharp
public static OcctModelLocation CreateTranslationLocation(double x, double y, double z)
```

### `CreateUniformScaleLocation`

```csharp
public static OcctModelLocation CreateUniformScaleLocation(double scale, OcctPoint3d center)
```

### `CreateUniformScaleTransform`

```csharp
public static OcctTransform3d CreateUniformScaleTransform(double scale, OcctPoint3d center)
```

### `Expanded`

```csharp
public static OcctBounds Expanded(OcctBounds bounds, double margin)
```

### `GetCenter`

```csharp
public static ValueTuple<double, double> GetCenter(OcctUvBounds bounds)
```

### `GetDiagonalLength`

```csharp
public static double GetDiagonalLength(OcctBounds bounds)
```

### `GetMaximumPoint`

```csharp
public static OcctPoint3d GetMaximumPoint(OcctBounds bounds)
```

### `GetMidpoint`

```csharp
public static OcctPoint3d GetMidpoint(OcctDistanceResult result)
```

### `GetMinimumPoint`

```csharp
public static OcctPoint3d GetMinimumPoint(OcctBounds bounds)
```

### `GetSeparationVector`

```csharp
public static OcctVector3d GetSeparationVector(OcctDistanceResult result)
```

### `GetVolume`

```csharp
public static double GetVolume(OcctBounds bounds)
```

### `Intersects`

```csharp
public static bool Intersects(OcctBounds first, OcctBounds second, double tolerance)
```

### `Inverted`

```csharp
public static OcctModelLocation Inverted(OcctModelLocation transform)
```

### `IsAffine`

```csharp
public static bool IsAffine(OcctModelLocation transform, double tolerance)
```

### `IsFinite`

```csharp
public static bool IsFinite(OcctBounds bounds)
```

### `IsFinite`

```csharp
public static bool IsFinite(OcctDistanceResult result)
```

### `IsFinite`

```csharp
public static bool IsFinite(OcctUvBounds bounds)
```

### `IsValid`

```csharp
public static bool IsValid(OcctBounds bounds)
```

### `IsValid`

```csharp
public static bool IsValid(OcctUvBounds bounds)
```

### `IsWithin`

```csharp
public static bool IsWithin(OcctDistanceResult result, double tolerance)
```

### `Lerp`

```csharp
public static OcctPoint3d Lerp(OcctPoint3d from, OcctPoint3d to, double amount)
```

### `Multiply`

Returns × . With column-vector semantics, is applied first and second.

```csharp
public static OcctModelLocation Multiply(OcctModelLocation left, OcctModelLocation right)
```

### `Multiply`

Returns × . With column-vector semantics, is applied first and second.

```csharp
public static OcctTransform3d Multiply(OcctTransform3d left, OcctTransform3d right)
```

### `ProjectOnto`

```csharp
public static OcctVector3d ProjectOnto(OcctVector3d vector, OcctVector3d axis)
```

### `RejectFrom`

```csharp
public static OcctVector3d RejectFrom(OcctVector3d vector, OcctVector3d axis)
```

### `ToModelLocation`

```csharp
public static OcctModelLocation ToModelLocation(OcctTransform3d transform)
```

### `ToTransform3d`

```csharp
public static OcctTransform3d ToTransform3d(OcctModelLocation transform)
```

### `TransformPoint`

```csharp
public static OcctPoint3d TransformPoint(OcctModelLocation transform, OcctPoint3d point)
```

### `TransformPoint`

```csharp
public static OcctPoint3d TransformPoint(OcctTransform3d transform, OcctPoint3d point)
```

### `TransformVector`

```csharp
public static OcctVector3d TransformVector(OcctModelLocation transform, OcctVector3d vector)
```

### `TransformVector`

```csharp
public static OcctVector3d TransformVector(OcctTransform3d transform, OcctVector3d vector)
```

### `TryInvert`

```csharp
public static bool TryInvert(OcctModelLocation transform, OcctModelLocation inverse)
```

### `Union`

```csharp
public static OcctBounds Union(OcctBounds first, OcctBounds second)
```

## Fields / Enum Values

None.

