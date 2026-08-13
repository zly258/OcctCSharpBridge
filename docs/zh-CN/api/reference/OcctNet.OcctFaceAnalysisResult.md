# OcctFaceAnalysisResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

无。

## 属性

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

## 事件

无。

## 方法

### `GetFacesBySurfaceType`

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> GetFacesBySurfaceType(OcctSurfaceType surfaceType)
```

## 字段 / 枚举值

无。

