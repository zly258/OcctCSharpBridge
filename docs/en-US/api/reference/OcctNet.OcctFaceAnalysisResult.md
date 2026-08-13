# OcctFaceAnalysisResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

None.

## Properties

### `FaceCount`

```csharp
public int FaceCount { get; }
```

### `Faces`

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> Faces { get; }
```

### `MaximumTolerance`

```csharp
public double MaximumTolerance { get; }
```

### `Root`

```csharp
public OcctModelShape Root { get; }
```

### `SurfaceTypeCounts`

```csharp
public IReadOnlyDictionary<OcctSurfaceType, int> SurfaceTypeCounts { get; }
```

### `TotalArea`

```csharp
public double TotalArea { get; }
```

## Events

None.

## Methods

### `GetFacesBySurfaceType`

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> GetFacesBySurfaceType(OcctSurfaceType surfaceType)
```

## Fields / Enum Values

None.

