# OcctFaceAnalysisResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctFaceAnalysisResult
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

None

## Properties

### `FaceCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int FaceCount { get; }
```

### `Faces`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> Faces { get; }
```

### `MaximumTolerance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double MaximumTolerance { get; }
```

### `Root`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Root { get; }
```

### `SurfaceTypeCounts`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyDictionary<OcctSurfaceType, int> SurfaceTypeCounts { get; }
```

### `TotalArea`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double TotalArea { get; }
```

## Events

None

## Methods

### `GetFacesBySurfaceType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> GetFacesBySurfaceType(OcctSurfaceType surfaceType)
```

**Parameters**

- `surfaceType` — `OcctSurfaceType`

**Returns:** `IReadOnlyList<OcctFaceAnalysisInfo>`

## Fields / Enum Values

None

