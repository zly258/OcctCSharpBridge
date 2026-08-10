# OcctFaceAnalysisResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctFaceAnalysisResult
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

无

## 属性

### `FaceCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int FaceCount { get; }
```

### `Faces`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> Faces { get; }
```

### `MaximumTolerance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double MaximumTolerance { get; }
```

### `Root`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Root { get; }
```

### `SurfaceTypeCounts`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyDictionary<OcctSurfaceType, int> SurfaceTypeCounts { get; }
```

### `TotalArea`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double TotalArea { get; }
```

## 事件

无

## 方法

### `GetFacesBySurfaceType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctFaceAnalysisInfo> GetFacesBySurfaceType(OcctSurfaceType surfaceType)
```

**参数**

- `surfaceType` — `OcctSurfaceType`

**返回值:** `IReadOnlyList<OcctFaceAnalysisInfo>`

## 字段 / 枚举值

无

